using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;
using MongoDB.Bson;

namespace MonJobs
{
    /// <summary>
    /// The unique id for a job.
    /// </summary>
    [ImmutableObject(true)]
    public sealed class JobId : IComparable<JobId>, IConvertible<ObjectId>, IConvertible<string>
    {
        private static readonly Regex ValidationRegex =
            new Regex(@"^[0-9a-fA-F]{24}$", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private readonly ObjectId _sourceValue;

        private JobId(string jobId)
        {
            Contract.Requires(jobId != null);
            Contract.Requires(jobId != string.Empty);
            Contract.Ensures(_sourceValue != null);
            if (string.IsNullOrWhiteSpace(jobId)) throw new ArgumentNullException(nameof(jobId));
            if (!ValidationRegex.IsMatch(jobId)) throw new ArgumentException($"The {nameof(jobId)} provided ({jobId}) is not properly formatted, we expect something that looks like a MongoDB ObjectId", nameof(jobId));
            _sourceValue = new ObjectId(jobId);
        }

        private JobId(ObjectId jobId)
        {
            Contract.Requires(jobId != null);
            Contract.Ensures(this._sourceValue != null);
            _sourceValue = jobId;
        }

        #region Equality

        private sealed class SourceValueEqualityComparer : IEqualityComparer<JobId>
        {
            public bool Equals(JobId x, JobId y)
            {
                if (ReferenceEquals(x, y)) return true;
                if (ReferenceEquals(x, null)) return false;
                if (ReferenceEquals(y, null)) return false;
                if (x.GetType() != y.GetType()) return false;
                return x._sourceValue.Equals(y._sourceValue);
            }

            public int GetHashCode(JobId obj)
            {
                return obj._sourceValue.GetHashCode();
            }
        }

        public static IEqualityComparer<JobId> SourceValueComparer { get; } = new SourceValueEqualityComparer();

        private bool Equals(JobId other)
        {
            return _sourceValue.Equals(other._sourceValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is JobId && Equals((JobId)obj);
        }

        public override int GetHashCode()
        {
            return _sourceValue.GetHashCode();
        }

        #endregion

        #region Parsing

        public static JobId Parse(string jobId)
        {
            JobId result;
            if (!JobId.TryParse(jobId, out result))
            {
                throw new ArgumentException("Invalid JobId: " + jobId, nameof(jobId));
            }
            return result;
        }

        public static bool TryParse(string jobId, out JobId result)
        {
            if (string.IsNullOrWhiteSpace(jobId) || !ValidationRegex.IsMatch(jobId))
            {
                result = JobId.Empty();
                return false;
            }
            result = new JobId(jobId);
            return true;
        }

        public static JobId Parse(ObjectId jobId)
        {
            JobId result;
            if (!JobId.TryParse(jobId, out result))
            {
                throw new ArgumentException("Invalid JobId: " + jobId, nameof(jobId));
            };
            return result;
        }

        public static bool TryParse(ObjectId jobId, out JobId result)
        {
            result = new JobId(jobId);
            return true;
        }

        #endregion

        public static JobId Empty()
        {
            return new JobId(ObjectId.Empty);
        }

        public int CompareTo(JobId other)
        {
            return _sourceValue.CompareTo(other._sourceValue);
        }

        ObjectId IConvertible<ObjectId>.ToValueType()
        {
            return _sourceValue;
        }

        string IConvertible<string>.ToValueType()
        {
            return _sourceValue.ToString();
        }

        public override string ToString()
        {
            return _sourceValue.ToString();
        }

        public static implicit operator ObjectId(JobId jobId)
        {
            return jobId?._sourceValue ?? ObjectId.Empty;
        }

        public static implicit operator string(JobId jobId)
        {
            return jobId?._sourceValue.ToString();
        }

        public static JobId Generate()
        {
            return new JobId(ObjectId.GenerateNewId());
        }
    }
}