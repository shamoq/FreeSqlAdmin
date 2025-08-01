namespace Simple.AdminApplication.TenantMng.Middleware
{
    /// <summary>
    /// 租户中间件扩展类
    /// </summary>
    public static class TenantMiddlewareExtensions
    {
        /// <summary>
        /// 使用租户中间件
        /// </summary>
        /// <param name="builder">应用程序构建器</param>
        /// <returns>应用程序构建器</returns>
        public static IApplicationBuilder UseTenantMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<TenantMiddleware>();
        }
    }
}