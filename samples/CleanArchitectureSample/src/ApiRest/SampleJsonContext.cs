using System.Text.Json.Serialization;
using App.Application.DTOs.Request;
using App.Application.DTOs.Response;
using App.Application.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using OrionSoft.AppCore.Application.Wrappers;
using App.Application.Features.Employes.Command;

namespace App.ApiRest;

[JsonSerializable(typeof(Response<EmployeResponseDto>))]
[JsonSerializable(typeof(Response<List<EmployeResponseDto>>))]
[JsonSerializable(typeof(Response<EmployeEntity>))]
[JsonSerializable(typeof(Response<List<EmployeEntity>>))]
[JsonSerializable(typeof(Response))]
[JsonSerializable(typeof(Response<List<PokemonEntity>>))]
[JsonSerializable(typeof(HttpResponse<PokemonResponse>))]
[JsonSerializable(typeof(PokemonResponse))]
[JsonSerializable(typeof(EmployeRequestDto))]
[JsonSerializable(typeof(EmployeResponseDto))]
[JsonSerializable(typeof(EmployeEntity))]
[JsonSerializable(typeof(PokemonEntity))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(CustomErrorResponse))]
[JsonSerializable(typeof(MappingErrorResponse))]
[JsonSerializable(typeof(AddEmployeCommand))]
[JsonSerializable(typeof(EditEmployeCommand))]
public partial class SampleJsonContext : JsonSerializerContext
{
}
