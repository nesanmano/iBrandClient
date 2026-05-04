namespace NomadLogic.Common.Util.Networking
{
    public class RestRequestJsonResponse<T> : RestRequestBasicResponse
    {
        public T InstancedBody { get; set; }
        
        public override string ToString()
        {
            return this.ToStringPublicProperties();
        }
    }
}