FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
#image: im:latest
COPY . ./
RUN for file in $(ls *.csproj); do mkdir -p ${file%.*}/ && mv $file ${file%.*}/; done
COPY *.sln ./
RUN dotnet restore
RUN dotnet build --no-restore -c Release