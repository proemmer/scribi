FROM microsoft/dotnet:latest

COPY ./Scribi /dotnetapp/Scribi
COPY ./Scribi.Attributes /dotnetapp/Scribi.Attributes
COPY ./Scribi.Interfaces /dotnetapp/Scribi.Interfaces
COPY ./global.json /dotnetapp
COPY ./NuGet.config /dotnetapp

WORKDIR /dotnetapp

RUN ["dotnet", "restore"]

RUN ["dotnet", "build" , "Scribi"]

CMD  ["dotnet", "run", "-p Scribi]
