FROM im:latest as build
WORKDIR /app
COPY . ./
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:5.0
WORKDIR /app
COPY --from=build /app/out/ .
ENTRYPOINT ["dotnet","IM.Gateways.Web.Companies.Api.dll"]