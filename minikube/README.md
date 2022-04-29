# PoC: Deploy CA WebApi using minikube

The goal of this PoC is to use devcontainer minikube definition to spawn CarbonAware WebApi service into a minikube cluster.

## Steps
1. Start minikube

    ```sh
    minikube start
    ```
1. Build CA WebApi service

    ```sh
    cd src/dotnet
    docker build -t cawebservice_image .
    ```

1. Deploy CA deployment yaml

    Since this deployment is using minikube, there is no need to upload the container's image to a docker registry (i.e docker.io). In order to use the local image builded previously (`cawebservice_image`) run on a terminal of the OS (i.e mac, linux) the following command:
    ```sh
    eval $(minikube docker-env) 
    ```
    After this let's deploy the service
    ```sh
    kubectl apply -f minikube/deployments/cawebservice-deployment.yml
    ```
1. List pods

    ```sh
    kubectl get pods
    NAME                                       READY   STATUS    RESTARTS   AGE
    cawebservice-deployment-66c746956b-7c7qs   1/1     Running   0          9s
    cawebservice-deployment-66c746956b-sfn4z   1/1     Running   0          9s
    ```
    There should be listed 2 pods, since the deployment descriptor has 2 replicas.

1. Deploy CA service yaml
    To access the Webservice, a `LoadBalancer` is created using `cawebservice-service.yml`. Before installing the service, it is important to enable minikube tunnel to be able to get an IP address to the service that can be reachable from the host. On a new terminal run:
    ```sh
    minikube tunnel
    ```
    Now deploy the service
    ```sh
    kubectl apply -f minikube/deployments/cawebservice-service.yml 
    ```

1. List Service

    ```sh
    kubectl get svc
    NAME           TYPE           CLUSTER-IP       EXTERNAL-IP      PORT(S)          AGE
    cawebservice   LoadBalancer   10.108.237.116   10.107.202.70   8080:30080/TCP   8s
    ```

1. Request WebService Endpoint

    ```sh
    curl "10.107.202.70:8080/emissions/bylocation?location=eastus" | jq
    ```

    ```json
    [
        {
            "location": "eastus",
            "time": "2022-04-22T20:45:11.5092664+00:00",
            "rating": 70
        },
        {
            "location": "eastus",
            "time": "2022-04-23T04:45:11.5092665+00:00",
            "rating": 38
        },
        {
            "location": "eastus",
            "time": "2022-04-23T12:45:11.5092666+00:00",
            "rating": 84
        },
        {
            "location": "eastus",
            "time": "2022-04-23T20:45:11.5092666+00:00",
            "rating": 75
        },
        ...
    ]
    ```

1. Delete CA Service

    ```sh
    kubectl delete -f minikube/deployments/cawebservice-deployment.yml
    kubectl delete -f minikube/deployments/cawebservice-service.yml 
    ```

## References

https://github.com/kubernetes/minikube/blob/0c616a6b42b28a1aab8397f5a9061f8ebbd9f3d9/README.md#reusing-the-docker-daemon
