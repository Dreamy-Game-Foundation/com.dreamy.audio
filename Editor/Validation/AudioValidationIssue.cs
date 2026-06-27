namespace Dreamy.Audio.Editor
{
    public readonly struct AudioValidationIssue
    {
        public AudioValidationIssue(AudioValidationSeverity severity, string code, string message)
        {
            Severity = severity;
            Code = code;
            Message = message;
        }

        public AudioValidationSeverity Severity { get; }
        public string Code { get; }
        public string Message { get; }
    }
}
