FROM mcr.microsoft.com/dotnet/core/aspnet:3.1
COPY ./out /out
WORKDIR /out
ENV ASPNETCORE_URLS http://*:80
ENV ASPNETCORE_ENVIRONMENT docker
ENTRYPOINT ["dotnet", "Condersp.Services.Orders.dll"]