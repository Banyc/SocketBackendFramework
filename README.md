# Socket Back-End Framework

## Architecture

### Relay Model

![architecture](img/arch.relay.drawio.png)

### Request-Response Model

(deprecated)

<details>
<summary></summary>

![architecture](img/arch.drawio.png)

</details>

## Features

-   Each configurable module has a clear responsibility.
-   Business logic decouples from transport infrastructure and pipelines.
-   It both supports the relay network model or the request-response network model.
-   Controllers can have different pipeline domains sent application messages.
-   It supports custom protocols in nature.

## How to use

The project `SocketBackendFramework.Relay.Sample` gives a simple example of using the framework. A new project can take this sample as a boilerplate.

## Components

-   Pipeline := a stack-like data structure containing several middlewares.
-   PipelineDomain := an virtual area including a pipeline and a dedicated transportMapper.
-   TransportAgent := a wrapped socket handler.
-   TransportMapper := an object that organizes many transportAgents.
    -   It is only configurable from config.json, not from user code.
-   PacketContext := an object that carries information and flows between a transportMapper and one of its transportAgents.
-   MiddlewareContext := an object that carries information and flows within a pipeline.
    -   Usually, a middlewareContext should bring a packetContext.
-   Workflow := a virtual area that owns a complete and independent back-end server.
    -   No data exchange is allowed between workflows.
-   WorkflowPool := a virtual area that collects all the running workflows. 
-   Controller := a object that majorly involves business logic.
