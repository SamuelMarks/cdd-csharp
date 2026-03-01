namespace Generated.Api
{
    using Microsoft.AspNetCore.Mvc;

    public interface IApi
    {
        [HttpGet("/pets")]
        void getPets();
    }
}