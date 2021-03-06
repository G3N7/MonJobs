# MonJobs
A [MongoDB](https://www.mongodb.com/) based jobs database

[![Build Status](https://travis-ci.org/G3N7/MonJobs.svg?branch=master)](https://travis-ci.org/G3N7/MonJobs)

## Services
Services are provided as Code APIs, REST APIs _(Coming Soon)_, and WebSocket APIs _(Coming Soon)_.

### Create
Jobs are created with a queue id, allowing for segmentation and a set of attributes.
[IJobCreationService] and [MongoJobCreationService] allows you to create a new [Job] by providing a [QueueId] and the [JobAttributes], then returns the new [Job].

### PeekNext
New Jobs can be viewed without effecting the queue by peeking at the next set of jobs that matching a given attribute query.  If one you chose to start a job you must first attempt an acknowledgment in order to ensure that it has not already been picked up.
[IJobPeekNextService] and [MongoJobPeekNextService] allows you to peek at the queue using criteria provided in [PeekNextOptions] once you have peeked you can chose to apply a [JobAcknowledgment] a [Job] and then do the work.

#### PeekNextOptions
An object that holds the query parameters.

* **QueueId**: [QueueId]
* **HasAttributes**: [JobAttributes] _(Optional)_
* **Limit**: `int?` _(Optional)_

### TakeNext
If you want to take the next job matching a given attribute query, this action will acknowledge the next available job and return its data.
[IJobTakeNextService] and [MongoJobTakeNextService] allows you to take the next item in the queue given the criteria provided in the [TakeNextOptions].

#### TakeNextOptions
An object that holds the options for the take feature.

* **QueueId**: [QueueId]
* **Acknowledgment**: [JobAcknowledgment]
* **HasAttributes**: [JobAttributes] _(Optional)_

### Query
Allows you to query the jobs of a queue by the criteria of a job query.
[IJobQueryService] and [MongoJobQueryService] make up the code API for seeing what jobs exist.  This query is made using the [JobQuery]

#### JobQuery
An object that holds the query parameters.

* **QueueId**: [QueueId] _(Optional)_
* **HasAttributes**: [JobAttributes] _(Optional)_
* **Limit**: `int?` _(Optional)_
* **HasBeenAcknowledged**: `bool?` _(Optional)_
* **HasResult**: `bool?` _(Optional)_

### Acknowledge
If you have seen a job that you want to start working, you can post an acknowledgment which will return true if still available, or false if the job is already acknowledged.
[IJobAcknowledgmentService] and [MongoJobAcknowledgmentService] allows you to apply a [JobAcknowledgment] to a [Job] and returns an [AcknowledgmentResult].

#### AcknowledgmentResult
The result of trying to acknowledge a job.
* **Success**: `bool`

### Report
You can update the job with reports about the progress.
[IJobReportService] and [MongoJobReportService] allow you to add as many [JobReport] to a [Job] as makes sense for your use-case.

### Complete
One you have completed a job you can call this to report a final result.
[IJobCompletionService] and [MongoJobCompletionService] allows you to add [JobResult] to a [Job].

## Types
Types are provided for static typing in _C#_, and serialize to BSON and JSON.

### Job
The record for the job as a whole, it consists of:
* **Id**: [JobId]
* **QueueId**: [QueueId]
* **Attributes**: [JobAttributes]
* **Acknowledgment**: [JobAcknowledgment]
* **Reports**: [JobReport[]][JobReport]
* **Result**: [JobResult]

### JobId
The unique id for a given job.
* BSON Representation - [ObjectId][OBJECT_ID]
* JSON Representation - [string][JSON_STRING]

### JobAttributes
A dictionary of attributes for this job.
* BSON Representation - [BsonDocument][BSON_DOCUMENT]
* JSON Representation - [object][JSON_OBJECT]

### JobAcknowledgment
A record of the acknowledgment.
* BSON Representation - [BsonDocument][BSON_DOCUMENT]
* JSON Representation - [object][JSON_OBJECT]

### JobReport
A message about the progress of a job.
* BSON Representation - [BsonDocument][BSON_DOCUMENT]
* JSON Representation - [object][JSON_OBJECT]

### JobResult
A record of the final result.
* BSON Representation - [BsonDocument][BSON_DOCUMENT]
* JSON Representation - [object][JSON_OBJECT]

### QueueId
An identity that provides the ability to segment the queue, this can be leveraged as an Authorization point.

* BSON Representation - [BsonString][BSON_STRING]
* JSON Representation - [string][JSON_STRING]

## Roadmap
Obviously this is just high level information about where the project is heading.

### Milestone Code (*Achieved 6/8*)
Get all services exposed via code API's functional with use-case based integration tests.

### Milestone REST
Get all of the services exposed via HTTP resource/verb strategy using a master API key.

### Milestone Docker
Get the rest API hosted in docker.

### Milestone Authorization Segmentation
Getting queues to only be available to certain entity/pass combos.

[BSON_DOCUMENT]: https://github.com/mongodb/mongo-csharp-driver/blob/master/src/MongoDB.Bson/ObjectModel/BsonDocument.cs
[BSON_STRING]: https://github.com/mongodb/mongo-csharp-driver/blob/master/src/MongoDB.Bson/ObjectModel/BsonString.cs
[OBJECT_ID]: https://github.com/mongodb/mongo-csharp-driver/blob/master/src/MongoDB.Bson/ObjectModel/ObjectId.cs

[JSON_STRING]: https://tools.ietf.org/html/rfc7159#section-7
[JSON_OBJECT]: https://tools.ietf.org/html/rfc7159#section-4

[JobResult]: MonJobs/JobResult.cs
[JobId]: MonJobs/JobId.cs
[Job]: MonJobs/Job.cs
[QueueId]: MonJobs/QueueId.cs
[JobAcknowledgment]: MonJobs/JobAcknowledgment.cs
[JobAttributes]: MonJobs/JobAttributes.cs
[JobReport]: MonJobs/JobReport.cs

[IJobCreationService]: MonJobs/IJobCreationService.cs
[MongoJobCreationService]: MonJobs/MongoJobCreationService.cs

[PeekNextOptions]: MonJobs/Peek/PeekNextOptions.cs
[IJobPeekNextService]: MonJobs/Peek/IJobPeekNextService.cs
[MongoJobPeekNextService]: MonJobs/Peek/MongoJobPeekNextService.cs

[IJobTakeNextService]: MonJobs/Take/MongoJobTakeNextService.cs
[MongoJobTakeNextService]: MonJobs/Take/MongoJobTakeNextService.cs
[TakeNextOptions]: MonJobs/Take/TakeNextOptions.cs

[IJobQueryService]: MonJobs/IJobQueryService.cs
[MongoJobQueryService]: MonJobs/MongoJobQueryService.cs
[JobQuery]: MonJobs/JobQuery.cs

[IJobAcknowledgmentService]: MonJobs/IJobAcknowledgmentService.cs
[MongoJobAcknowledgmentService]: MonJobs/MongoJobAcknowledgmentService.cs
[AcknowledgmentResult]: MonJobs/AcknowledgmentResult.cs

[IJobCompletionService]: MonJobs/IJobCompletionService.cs
[MongoJobCompletionService]: MonJobs/IJobCompletionService.cs

[IJobReportService]: MonJobs/IJobReportService.cs
[MongoJobReportService]: MonJobs/MongoJobReportService.cs
