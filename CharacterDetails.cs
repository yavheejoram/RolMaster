namespace RolMaster.Models;
public sealed class CharacterDetails
{
 public string Race {get;set;}="";
 public string CharacterClass {get;set;}="";
 public int Level {get;set;}=1;
 public Dictionary<string,string> Attributes {get;set;}=new();
 public List<string> Inventory {get;set;}=new();
 public List<string> Links {get;set;}=new();
 public List<string> History {get;set;}=new();
}
