var builder = DistributedApplication.CreateBuilder(args);
var cache = builder.AddRedis("cache")
	.WithRedisInsight()
	.WithDataVolume(isReadOnly: false)
	.WithPersistence(
		interval: TimeSpan.FromMinutes(1),
		keysChangedThreshold: 1); ; ;
var pgSql = builder.AddPostgres("db");

builder.AddProject<Projects.ConsoleRunner>("consolerunner")
	.WithReference(cache);

builder.Build().Run();
