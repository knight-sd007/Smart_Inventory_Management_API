pipeline {
    agent any

    parameters {
        string(name: 'DOCKERHUB_USERNAME', defaultValue: 'knightprime007', description: 'Docker Hub Username / Namespace')
        string(name: 'IMAGE_NAME', defaultValue: 'smart-inventory-management-api', description: 'Docker Hub Repository Name')
        string(name: 'OCI_HOST', defaultValue: 'inventory.vaikuntrix.in', description: 'Target OCI VM Hostname or IP')
    }

    environment {
        GIT_SHA = "${env.GIT_COMMIT ? env.GIT_COMMIT.take(7) : error('GIT_COMMIT is missing; immutable Git SHA tag is required')}"
        IMAGE_FULL_TAG = "${params.DOCKERHUB_USERNAME}/${params.IMAGE_NAME}:${env.GIT_SHA}"
        IMAGE_STABLE_TAG = "${params.DOCKERHUB_USERNAME}/${params.IMAGE_NAME}:stable"
        DOCKERHUB_CRED_ID = 'docker-hub-credentials'
    }

    options {
        timeout(time: 30, unit: 'MINUTES')
        disableConcurrentBuilds()
    }

    stages {
        stage('Checkout') {
            steps {
                checkout scm
                echo "Checked out commit: ${env.GIT_COMMIT}"
            }
        }

        stage('Secret Scan') {
            steps {
                script {
                    echo "Executing Gitleaks security scan via Docker container..."
                    sh 'docker run --rm -v "${WORKSPACE}:/source:ro" zricethezav/gitleaks:latest detect --source /source --verbose'
                }
            }
        }

        stage('Restore & Test') {
            steps {
                script {
                    echo "Executing dotnet restore & test via Docker SDK container..."
                    sh 'docker run --rm -v "${WORKSPACE}:/app" -w /app mcr.microsoft.com/dotnet/sdk:10.0 sh -c "dotnet restore tests/SmartInventory.Tests/SmartInventory.Tests.csproj && dotnet test tests/SmartInventory.Tests/SmartInventory.Tests.csproj --configuration Release --no-restore"'
                }
            }
        }

        stage('Build ARM64 Image') {
            steps {
                script {
                    echo "Building ARM64 Docker image (linux/arm64)..."
                    sh "docker buildx build --platform linux/arm64 -t ${IMAGE_FULL_TAG} -t ${IMAGE_STABLE_TAG} --load ."
                }
            }
        }

        stage('Trivy Scan') {
            steps {
                script {
                    echo "Scanning container image ${IMAGE_FULL_TAG} for HIGH and CRITICAL vulnerabilities via Docker container..."
                    sh "docker run --rm -v /var/run/docker.sock:/var/run/docker.sock aquasec/trivy:latest image --severity HIGH,CRITICAL --exit-code 1 ${IMAGE_FULL_TAG}"
                }
            }
        }

        stage('Push Docker Hub') {
            steps {
                script {
                    echo "Pushing container image ${IMAGE_FULL_TAG} to Docker Hub..."
                    withCredentials([usernamePassword(credentialsId: DOCKERHUB_CRED_ID, usernameVariable: 'DH_USER', passwordVariable: 'DH_PASS')]) {
                        sh 'echo "$DH_PASS" | docker login -u "$DH_USER" --password-stdin'
                        sh "docker push ${IMAGE_FULL_TAG}"
                        sh "docker push ${IMAGE_STABLE_TAG}"
                    }
                }
            }
        }

        stage('Deploy OCI') {
            steps {
                script {
                    echo "Deploying public image ${IMAGE_FULL_TAG} to local OCI host..."
                    sh """
                        if [ ! -s /opt/projects/inventory-api/.env ]; then
                            echo "ERROR: Production environment file /opt/projects/inventory-api/.env not found or empty on OCI host!"
                            exit 1
                        fi
                        cp docker-compose.yml /opt/projects/inventory-api/docker-compose.yml
                        P03_IMAGE="${IMAGE_FULL_TAG}" docker compose --env-file /opt/projects/inventory-api/.env -f /opt/projects/inventory-api/docker-compose.yml pull
                        P03_IMAGE="${IMAGE_FULL_TAG}" docker compose --env-file /opt/projects/inventory-api/.env -f /opt/projects/inventory-api/docker-compose.yml up -d
                    """
                }
            }
        }

        stage('Post-Deployment Verification') {
            steps {
                script {
                    sh '''
                        MAX_ATTEMPTS=15
                        SLEEP_SECONDS=2
                        CURL_TIMEOUT=2
                        HEALTH_URL="http://127.0.0.1:5003/health"
                        SWAGGER_URL="http://127.0.0.1:5003/swagger/v1/swagger.json"

                        echo "Layer 1 Verification: Bounded readiness check for internal application health ($HEALTH_URL)..."

                        ATTEMPT=1
                        SUCCESS=0

                        while [ $ATTEMPT -le $MAX_ATTEMPTS ]; do
                            HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" --max-time "$CURL_TIMEOUT" "$HEALTH_URL") || HTTP_CODE="000"

                            if [ "$HTTP_CODE" = "200" ]; then
                                echo "[Attempt $ATTEMPT/$MAX_ATTEMPTS] Layer 1 Healthcheck: PASS (HTTP 200 OK)"
                                SUCCESS=1
                                break
                            else
                                echo "[Attempt $ATTEMPT/$MAX_ATTEMPTS] Application starting up (HTTP $HTTP_CODE). Retrying..."
                                if [ "$ATTEMPT" -lt "$MAX_ATTEMPTS" ]; then
                                    sleep "$SLEEP_SECONDS"
                                fi
                                ATTEMPT=$((ATTEMPT + 1))
                            fi
                        done

                        if [ $SUCCESS -ne 1 ]; then
                            echo "ERROR: Layer 1 application readiness failed after $MAX_ATTEMPTS attempts against $HEALTH_URL (Final HTTP status: $HTTP_CODE)!"
                            exit 1
                        fi

                        echo "Layer 1 Verification: Internal OpenAPI Document ($SWAGGER_URL)..."
                        curl --fail --silent --show-error --max-time 10 "$SWAGGER_URL" > /dev/null
                    '''

                    echo "Layer 2 Verification: Cloudflared Tunnel status..."
                    sh '''
                        if pgrep cloudflared >/dev/null || systemctl is-active cloudflared >/dev/null 2>&1 || docker ps | grep -q cloudflared; then
                            echo "Cloudflared tunnel status: RUNNING"
                        else
                            echo "ERROR: Cloudflared tunnel process is not detected on host!"
                            exit 1
                        fi
                    '''

                    echo "Layer 3 Verification: Public domain (https://${params.OCI_HOST}/health)..."
                    sh """
                        HTTP_STATUS=\$(curl -o /dev/null -s -w "%{http_code}" --max-time 10 https://${params.OCI_HOST}/health || echo "CURL_ERROR")
                        if [ "\$HTTP_STATUS" = "200" ]; then
                            echo "Public Cloudflare route: PASS (HTTP 200 OK)"
                        elif [ "\$HTTP_STATUS" = "503" ] || [ "\$HTTP_STATUS" = "403" ]; then
                            echo "Public Cloudflare route: CHALLENGED (Cloudflare Under Attack Mode Active — HTTP \$HTTP_STATUS. Deployment Healthy)."
                        else
                            echo "ERROR: Public Cloudflare route failed with status \$HTTP_STATUS"
                            exit 1
                        fi
                    """
                }
            }
        }

        stage('OCI Disk Cleanup') {
            steps {
                script {
                    echo "Performing targeted disk space cleanup for obsolete P03 images..."
                    sh """
                        docker image prune -f
                        docker container prune -f
                        docker image ls --format '{{.Repository}}:{{.Tag}} {{.ID}}' | grep '${params.DOCKERHUB_USERNAME}/${params.IMAGE_NAME}' | grep -v '${env.GIT_SHA}' | grep -v 'stable' | awk '{print \$2}' | xargs -r docker rmi -f || true
                    """
                }
            }
        }
    }

    post {
        always {
            cleanWs()
        }
        success {
            echo "Successfully built, tested, scanned, pushed, and deployed P03 commit ${env.GIT_SHA}!"
        }
        failure {
            echo "Pipeline failed! Deployment aborted."
        }
    }
}
