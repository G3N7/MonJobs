# MonJobs
A Mongo based jobs database

[![Build Status](https://travis-ci.org/G3N7/MonJobs.svg?branch=master)](https://travis-ci.org/G3N7/MonJobs)

## Services
Services are provided as Code APIs, REST APIs (Coming Soon), and WebSocket APIs (Coming Soon).

### Create
Jobs are created with a queue id, allowing for segmentation and a set of attributes.
`IJobCreationService` and `MongoJobCreationService` make up the code API for creating a new job.

### PeekNext
New Jobs can be viewed without effecting the queue by peeking at the next set of jobs that matching a given attribute query.  If one you chose to start a job you must first attempt an acknowledgment in order to ensure that it has not already been picked up.
`IJobPeekNextService` and `MongoJobPeekNextService` make up the code API for seeing what jobs are next.

### TakeNext
If you want to take the next job matching a given attribute query, this action will acknowledge the next available job and return its data.

### Acknowledgment
If you have seen a job that you want to start working, you can post an acknowledgment which will return true if still available, or false if the job is already acknowledged.

### Report
You can update the job with reports about the progress.

### Completion
One you have completed a job you can call this to report a final result.
`IJobCompletionService` and `MongoJobCompletionService` make up the code API for completing a job.

## Types
Types are provided for static typing in C#, and serializable to Bson and Json.

### JobId
The unique id for a given job.

#### Bson Representation
ObjectId

#### Json Representation
String

### JobAttributes
A dictionary of attributes for this job.

#### Bson Representation
BsonDocument

#### Json Representation
Object

### JobAcknowledgment