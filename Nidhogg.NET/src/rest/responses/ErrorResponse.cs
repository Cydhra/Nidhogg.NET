// ReSharper disable InconsistentNaming
// because naming is linked to JSON response naming

namespace Nidhogg.rest.responses
{
    /// <summary>
    /// A Yggdrasil error response
    /// </summary>
    public struct ErrorResponse
    {
        public string error { get; set; }
        public string errorMessage { get; set; }
        public string cause { get; set; }
    }
}