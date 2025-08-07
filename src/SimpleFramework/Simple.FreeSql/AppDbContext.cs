using Simple.Utils.Helper;

namespace Simple.FreeSql
{
    public abstract class AppDbContext
    {
        public bool UnitOfWork { protected get; set; }

        protected readonly string connectString;

        public AppDbContext(string connectString)
        {
            this.connectString = connectString;
        }
    }
}