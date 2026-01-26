using System.Text.Json.Serialization;
using App.Application.DTOs.Request;
using App.Application.DTOs.Response;
using App.Application.Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using AppCore.Application.Wrappers;

namespace App.ApiRest;

[JsonSerializable(typeof(Response<EmployeResponseDto>))]
[JsonSerializable(typeof(Response<List<EmployeResponseDto>>))]
[JsonSerializable(typeof(Response<EmployeEntity>))]
[JsonSerializable(typeof(Response<List<EmployeEntity>>))]
[JsonSerializable(typeof(Response<bool>))]
[JsonSerializable(typeof(Response<List<PokemonEntity>>))]
[JsonSerializable(typeof(EmployeRequestDto))]
[JsonSerializable(typeof(EmployeResponseDto))]
[JsonSerializable(typeof(EmployeEntity))]
[JsonSerializable(typeof(PokemonEntity))]
[JsonSerializable(typeof(ProblemDetails))]
[JsonSerializable(typeof(ValidationProblemDetails))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(CustomErrorResponse))]
[JsonSerializable(typeof(MappingErrorResponse))]
public partial class SampleJsonContext : JsonSerializerContext
{
}
