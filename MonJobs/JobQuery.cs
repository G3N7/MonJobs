namespace MonJobs
{
    // doc: JobQuery
    public class JobQuery
    {
        public QueueId QueueId { get; set; }

        public JobId[] JobIds { get; set; }

        private JobAttributes _hasAttributes;
        public JobAttributes HasAttributes
        {
            get { return _hasAttributes ?? (_hasAttributes = new JobAttributes()); }
            set { _hasAttributes = value; }
        }

        public string AdhocQuery { get; set; }
        public string AdhocSort { get; set; }

        // mitch: these should be value types instead of nullables.
        public int? Limit { get; set; }
        public int? Skip { get; set; }
        public bool? HasBeenAcknowledged { get; set; }
        public bool? HasResult { get; set; }
    }
}