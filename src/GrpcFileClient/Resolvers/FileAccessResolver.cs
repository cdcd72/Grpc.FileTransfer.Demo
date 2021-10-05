using GrpcFileClient.Types;
using Infra.Core.FileAccess.Abstractions;

namespace GrpcFileClient.Resolvers
{
    public delegate IFileAccess FileAccessResolver(FileAccessType fileAccessType);
}
