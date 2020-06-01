# serverless-sample-code
Serverless sample code using Azure functions, durable functions, stateful entities, service bus queue, event hub, stream analytics, and cosmos db (SQL API).

### Visual Studio 2019 Solution
#### DataGen.FuncApp
Azure function app project consisting of standard functions as well as durable functions, and, stateful entities.
##### Key Functions
* **ServiceBusTrigger**
Receives messages from a service bus queue and signals ScanDeviceDispatcher.
* **ScanDeviceDispatcher**
Abstracts a discrete customer scan device. Requests are forwarded to ScanDeviceWorker instances to allow controlled, parallel processing.
* **ScanDeviceWorker**
Interacts with a discreet customer scan device - submits http requests to devices and sends results to an event hub.
* **ScanScheduler**
An example of how to invoke stateful entities via HTTP trigger - refer to _ScanDevice_ method.

#### DataGen.ConsoleApp
Console app used to generate and sent messages to service bus queue. Execute from command line:
```
DataGen.ConsoleApp.exe -d [number-of-devices] -c [number-of-customers] -i [number-of-iterations] -s [seconds-between-iterations]
```
Parameter defaults:
* d: 1
* c: 1
* i: 1
* s: 30

### Running this code
This code depends on the following services:
* Azure Service Bus queue
* Azure Event Hub

The Azure Function App can run locally, or, in Azure. To run locally, install [Azure Storage Emulator](https://docs.microsoft.com/en-us/azure/storage/common/storage-use-emulator). Alternatively, create an Azure Function App and deploy the Function App project to this instance.

The solution must be built locally using Visual Studio 2019, or, using [Azure DevOps](https://docs.microsoft.com/en-us/azure/devops/pipelines/targets/azure-functions?view=azure-devops&tabs=dotnet-core%2Cyaml).

#### Persisting Messages to Cosmos DB
Optionally, records from the event hub can be persisted to Cosmos DB (SQL API) through Azure Stream Analytics.

