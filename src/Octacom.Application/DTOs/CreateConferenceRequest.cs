using Octacom.Domain;

namespace Octacom.Application.DTOs;

public class CreateConferenceRequest
{
    public string Name { get; set; }= string.Empty;
    public int TotalCapacity { get; set; }
}