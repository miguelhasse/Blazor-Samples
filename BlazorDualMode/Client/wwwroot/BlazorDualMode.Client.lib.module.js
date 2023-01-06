// https://learn.microsoft.com/en-us/aspnet/core/blazor/fundamentals/startup?view=aspnetcore-7.0#javascript-initializers

export function beforeStart(options, extensions) {
    console.log("beforeStart");
}

export function afterStarted(blazor) {
    console.log("afterStarted");
}