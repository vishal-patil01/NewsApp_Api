namespace NewsApp.API.Middlewares;

public static class SecurityHeadersMiddleware
{
    public static void UseSecurityHeader(this IApplicationBuilder app)
    {
        app.UseCsp(options =>
        {
            options.BlockAllMixedContent()
            .ScriptSources(s => s.Self())
            .StyleSources(s => s.Self())
            .StyleSources(s => s.UnsafeInline())
            .FontSources(s => s.Self())
            .FormActions(s => s.Self())
            .FrameAncestors(s => s.Self())
            .ImageSources(s => s.Self());
        });
        app.UseXfo(option =>
        {
            option.Deny();
        });
        app.UseXXssProtection(option =>
        {
            option.EnabledWithBlockMode();
        });
        app.UseXContentTypeOptions();
        app.UseReferrerPolicy(opts => opts.NoReferrer());

        app.Use(async (context, next) =>
        {
            context.Response.Headers.Add("Strict-Transport-Security", "max-age=31536000");
            await next.Invoke();
        });
    }
}
