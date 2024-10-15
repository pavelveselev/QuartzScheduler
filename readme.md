A simple http reminder. Contains an API that accepts CRUD requests for tasks in the scheduler. Based on [Quartz.Net library](https://github.com/quartznet/quartznet).

The QuartzScheduler.Api project is the entry point and the service itself.

This is a stateful service, there is a worker QuartzHostedService that raises/stops the Quartz scheduler. It is not recommended to deploy on IIS. Better in docker, but if there is a need to deploy on Windows, it is better to deploy it as a service, but then you must add the .UseWindowsService() line in Program.cs

The entry point is the ScheduleController, which allows you to add/delete/receive tasks in the scheduler. At the specified time, sends a request to the URL specified when creating the task. Calls of POST,PUT,GET, DELETE requests from the scheduler are implemented.

Quartz starts tasks in UTC time. That is, if necessary, run using a cron expression in local time, you need to explicitly specify the timezone in the timezoneOffset field (optional, TimeSpan). The service finds the appropriate timezone among the system timezones and puts the task in the scheduler with the appropriate offset. If it does not find the system timezone, it will issue a BadRequest.

API versions are implemented:
1. Adding a task for triggering by crown expression or once in time. When created, it returns the task ID of the shadower in response, which can then be used to get information about the task or delete it. When triggered, the URL specified in the model is called.
2. The same as version 1, but you can specify the number and period of repetitions in case of an unsuccessful response from the server (when the task was triggered, the URL was pulled, and the server did not respond with 2xx). The key difference in the second version is that the task ID is not returned to the outside. An ObjectId (external identifier) and an applicationId are passed to ensure the uniqueness of the ObjectId-applicationId bundle within the framework of a shader that works for several services.

In appsettings.Development.The json configuration is empty, everything is by default, i.e. tasks are stored in memory. In this case, the ObjectId-applicationId bundle is also stored in memory. I.e., if you run the console in the debug, there will be an isolated scheduler that does not depend on anything, and your cars will work while the process is running.
In appsettings.prerelease.json example of the Quartz configuration with saving to the MSSQL database and clustering. About clustering: https://www.quartz-scheduler.net/documentation/quartz-3.x/tutorial/advanced-enterprise-features.html#clustering
Database tables creation script is in scripts/tables_sqlServer.sql. Original: https://raw.githubusercontent.com/quartznet/quartznet/main/database/tables/tables_sqlServer.sql
In this case, the ObjectId-applicationId bundle is also stored in the database.
In the case of clustering on multiple machines, it is necessary that the time on the machines diverge by no more than a second. Clustering works on the basis of a common database.
All Quartz settings are hardcoded in Startup.cs.
