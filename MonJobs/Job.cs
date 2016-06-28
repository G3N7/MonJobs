namespace MonJobs
{
    // doc: Job
    public class Job
    {
        public JobId Id { get; set; }
        public QueueId QueueId { get; set; }

        private JobAttributes _attributes;
        public JobAttributes Attributes
        {
            get { return _attributes ?? (_attributes = new JobAttributes()); }
            set { _attributes = value; }
        }

        public JobAcknowledgment Acknowledgment { get; set; }

        private JobReport[] _reports;
        public JobReport[] Reports
        {
            get { return _reports ?? (_reports = new JobReport[0]); }
            set { _reports = value; }
        }

        public JobResult Result { get; set; }
    }
}
