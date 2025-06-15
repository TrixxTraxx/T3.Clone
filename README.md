# LLMLab

This project is a web-based application for interacting with large language models (LLMs). It allows users to create and manage message threads, send messages, and receive responses from different AI models.

## Running the Project

Prerequisites:
*   .NET SDK (version 9.0 or later)
*   WASM Tools (for client-side WebAssembly support https://learn.microsoft.com/en-us/aspnet/core/blazor/webassembly-build-tools-and-aot?view=aspnetcore-9.0)
*   Docker (for running the MSSQL database)


1.  Clone the repository.
2.  Navigate to the `LLMLab.AppHost` directory.
3.  Run the command `dotnet run`.

This will start the application, including the Database, server and client.

4.  Open your web browser and navigate to `http://localhost:5000` to access the application.

## Deploying the Project

To deploy the project, you need to publish the `LLMLab.Server` and `LLMLab.ClientWebServer` projects.

1.  Publish the server:
    ```bash
    dotnet publish LLMLab.Server -c Release
    ```
2.  Publish the client project:
    ```bash
    dotnet publish LLMLab.Client -c Release
    ```
you can add the -o option to specify the output directory for the published files, for example:
    ```bash
    dotnet publish LLMLab.Client -c Release -o ./publish
    ```
3.  (Optional) if you dont have a Server to host the client files you can use the `LLMLab.ClientWebServer` project to serve the client files. Publish it as well:
    ```bash
    dotnet publish LLMLab.ClientWebServer -c Release
    ```

You can then deploy the published artifacts to your hosting environment.

## Database Model

![img_1.png](img_1.png)

There are also some Tables related to Asp.Net Core Identity and Hangfire but they are managed by libraries!

## Sync Models

The application uses SignalR for real-time communication between the server and the client. The following models are used for synchronization:

### Message Threads

![img_2.png](img_2.png)

### Message Streaming

![img_3.png](img_3.png)

### Settings

Settings to not get syncronised or stored on the Server, they are using a Key-Value store in the browser (localStorage) to persist the settings across sessions.