#image: im:latest

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
COPY . ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
COPY *.sln ./
RUN dotnet restore
RUN dotnet build --no-restore -c Release