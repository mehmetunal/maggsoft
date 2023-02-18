using System;
using System.IO;
using Maggsoft.Core.Base;
using System.Threading.Tasks;

namespace Maggsoft.Framework.Helper.ResponseRewind
{
    public interface IExtensionsHelper
    {
        Task Bind(Stream body, Response<object> response, Exception ex);
    }
}
