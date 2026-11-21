using Microsoft.Data.Sqlite;
using RolMaster.Models;
namespace RolMaster.Services;
public sealed class Database
{
 public string PathOnDisk { get; }
 private string ConnectionString => new SqliteConnectionStringBuilder { DataSource = PathOnDisk }.ToString();
 public Database()
 {
  var folder = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),"RolMaster");
  Directory.CreateDirectory(folder);
  PathOnDisk = System.IO.Path.Combine(folder,"rolmaster.db");
  using var c = Open(); using var cmd = c.CreateCommand();
  cmd.CommandText = "CREATE TABLE IF NOT EXISTS records(id TEXT PRIMARY KEY,kind TEXT NOT NULL,title TEXT NOT NULL,campaign_id TEXT NOT NULL,description TEXT NOT NULL,system TEXT NOT NULL,data TEXT NOT NULL,updated_utc TEXT NOT NULL); CREATE INDEX IF NOT EXISTS idx_records_kind ON records(kind);";
  cmd.ExecuteNonQuery();
 }
 private SqliteConnection Open() { var c = new SqliteConnection(ConnectionString); c.Open(); return c; }
 public List<Record> Get(string kind)
 {
  using var c=Open(); using var cmd=c.CreateCommand(); cmd.CommandText="SELECT id,kind,title,campaign_id,description,system,data,updated_utc FROM records WHERE kind=$kind ORDER BY title COLLATE NOCASE"; cmd.Parameters.AddWithValue("$kind",kind);
  using var r=cmd.ExecuteReader(); var result=new List<Record>();
  while(r.Read()) result.Add(new Record {Id=r.GetString(0),Kind=r.GetString(1),Title=r.GetString(2),CampaignId=r.GetString(3),Description=r.GetString(4),System=r.GetString(5),Data=r.GetString(6),UpdatedUtc=DateTime.Parse(r.GetString(7),null,System.Globalization.DateTimeStyles.RoundtripKind)});
  return result;
 }
 public void Save(Record record)
 {
  record.UpdatedUtc=DateTime.UtcNow;
  using var c=Open();using var cmd=c.CreateCommand();
  cmd.CommandText="INSERT INTO records(id,kind,title,campaign_id,description,system,data,updated_utc) VALUES($id,$kind,$title,$campaign,$description,$system,$data,$updated) ON CONFLICT(id) DO UPDATE SET title=excluded.title,campaign_id=excluded.campaign_id,description=excluded.description,system=excluded.system,data=excluded.data,updated_utc=excluded.updated_utc";
  cmd.Parameters.AddWithValue("$id",record.Id);cmd.Parameters.AddWithValue("$kind",record.Kind);cmd.Parameters.AddWithValue("$title",record.Title);cmd.Parameters.AddWithValue("$campaign",record.CampaignId);cmd.Parameters.AddWithValue("$description",record.Description);cmd.Parameters.AddWithValue("$system",record.System);cmd.Parameters.AddWithValue("$data",record.Data);cmd.Parameters.AddWithValue("$updated",record.UpdatedUtc.ToString("O"));cmd.ExecuteNonQuery();
 }
 public void Delete(string id) { using var c=Open();using var cmd=c.CreateCommand();cmd.CommandText="DELETE FROM records WHERE id=$id";cmd.Parameters.AddWithValue("$id",id);cmd.ExecuteNonQuery(); }
 public void Import(IEnumerable<Record> records)
 {
  using var c=Open();using var tx=c.BeginTransaction();
  foreach(var record in records) { using var cmd=c.CreateCommand();cmd.Transaction=tx;cmd.CommandText="INSERT OR REPLACE INTO records(id,kind,title,campaign_id,description,system,data,updated_utc) VALUES($id,$kind,$title,$campaign,$description,$system,$data,$updated)";cmd.Parameters.AddWithValue("$id",record.Id);cmd.Parameters.AddWithValue("$kind",record.Kind);cmd.Parameters.AddWithValue("$title",record.Title);cmd.Parameters.AddWithValue("$campaign",record.CampaignId);cmd.Parameters.AddWithValue("$description",record.Description);cmd.Parameters.AddWithValue("$system",record.System);cmd.Parameters.AddWithValue("$data",record.Data);cmd.Parameters.AddWithValue("$updated",record.UpdatedUtc.ToString("O"));cmd.ExecuteNonQuery(); }
  tx.Commit();
 }
}
