using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
public class Todo
{
    public int Id { get; set; }
    public string text { get; set; }
    public int UserId { get; set; }
    public bool done { get; set; }
    public DateTime time { get; set; }
    [JsonIgnore]
    [ValidateNever]
    public User User { get; set; }
}