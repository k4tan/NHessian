using System.Threading.Tasks;

namespace com.caucho.hessian.test
{
    public interface ITestService
    {
        Task nullCall();

        Task<string> hello();

        Task<int> subtract(int a, int b);

        Task<object> echo(object value);

        Task fault();
    }
}
