var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/sse", async ctx =>
{
    var hosting = ctx.RequestServices.GetRequiredService<IHostApplicationLifetime>();
    using var requestOrServerAbort = CancellationTokenSource.CreateLinkedTokenSource(
        ctx.RequestAborted,
        hosting.ApplicationStopping);

    ctx.Response.Headers.Append("Content-Type", "text/event-stream; charset=utf-8");
    ctx.Response.Headers.Append("Cache-Control", "no-cache");

    do
    {
        await ctx.Response.WriteAsync(
            $"data: Server time {DateTime.Now}\n\n",
            ctx.RequestAborted);

    } while (await SleepUntilNextMessage(requestOrServerAbort.Token));
});

async Task<bool> SleepUntilNextMessage(CancellationToken token)
{
    try
    {
        await Task.Delay(TimeSpan.FromSeconds(1), token);
        return true;
    }
    catch (TaskCanceledException)
    {
        return false;
    }
}

app.Run();