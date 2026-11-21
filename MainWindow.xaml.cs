using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using RolMaster.Models;
using RolMaster.Services;
namespace RolMaster;
public partial class MainWindow : Window
{
 private readonly Database db = new();
 private readonly string[] systems = {"AD&D 2.ª edición","D&D 3.5 / D20","D&D 5.ª edición","Pathfinder","Mundo de Tinieblas","La llamada de Cthulhu","El Señor de los Anillos","Personalizado"};
 private readonly Random rng=new();
 public MainWindow(){InitializeComponent();ShowRecords("campaign","Campañas");}
 private void Campaigns_Click(object s,RoutedEventArgs e)=>ShowRecords("campaign","Campañas");
 private void Characters_Click(object s,RoutedEventArgs e)=>ShowRecords("character","Personajes");
 private void Adventures_Click(object s,RoutedEventArgs e)=>ShowRecords("adventure","Aventuras");
 private void Maps_Click(object s,RoutedEventArgs e)=>ShowMaps();
 private void Director_Click(object s,RoutedEventArgs e)=>ShowDirector();
 private void Library_Click(object s,RoutedEventArgs e)=>ShowRecords("library","Biblioteca");
 private void Backup_Click(object s,RoutedEventArgs e)=>ShowBackup();
 private static TextBlock Label(string text)=>new(){Text=text,Foreground=Brushes.BurlyWood,Margin=new Thickness(4,12,0,3)};
 private static TextBox Input(string text="",int height=32)=>new(){Text=text,MinHeight=height,AcceptsReturn=height>60,TextWrapping=TextWrapping.Wrap,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
 private static Button Action(string title,Action callback){var b=new Button{Content=title};b.Click+=(_,_)=>callback();return b;}
 private void ShowRecords(string kind,string title)
 {
  PageTitle.Text=title;
  var grid=new Grid();grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(300)});grid.ColumnDefinitions.Add(new ColumnDefinition{Width=new GridLength(1,GridUnitType.Star)});
  var left=new DockPanel{Margin=new Thickness(0,0,15,0)};var list=new ListBox{ItemsSource=db.Get(kind)};
  var create=Action("+ Nuevo",()=>{var record=new Record{Kind=kind,Title="Nuevo " + (kind=="character"?"personaje":kind=="campaign"?"campaña":"registro")};db.Save(record);ShowRecords(kind,title);});DockPanel.SetDock(create,Dock.Top);left.Children.Add(create);left.Children.Add(list);grid.Children.Add(left);
  var editor=new StackPanel{Margin=new Thickness(10,0,0,0)};var editorScroll=new ScrollViewer{Content=editor,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};Grid.SetColumn(editorScroll,1);grid.Children.Add(editorScroll);
  void Edit(Record? record)
  {
   editor.Children.Clear();if(record==null){editor.Children.Add(Label("Selecciona un registro o crea uno nuevo."));return;}
   editor.Children.Add(Label("Nombre"));var name=Input(record.Title);editor.Children.Add(name);
   editor.Children.Add(Label("Campaña vinculada"));var campaigns=db.Get("campaign");var campaign=new ComboBox{ItemsSource=campaigns,DisplayMemberPath="Title",SelectedValuePath="Id"};campaign.SelectedValue=record.CampaignId;editor.Children.Add(campaign);
   ComboBox? system=null;
   if(kind=="character") {editor.Children.Add(Label("Sistema de rol"));system=new ComboBox{ItemsSource=systems,SelectedItem=record.System};editor.Children.Add(system);}
   editor.Children.Add(Label("Descripción, historia y notas"));var description=Input(record.Description,170);editor.Children.Add(description);
   TextBox? race=null, charClass=null, level=null, attributes=null, inventory=null, links=null, history=null;
   if(kind=="character")
   {
    CharacterDetails details;
    try { details=JsonSerializer.Deserialize<CharacterDetails>(record.Data)??new CharacterDetails(); }
    catch { details=new CharacterDetails(); }
    editor.Children.Add(Label("Raza / especie"));race=Input(details.Race);editor.Children.Add(race);
    editor.Children.Add(Label("Clase / profesión"));charClass=Input(details.CharacterClass);editor.Children.Add(charClass);
    editor.Children.Add(Label("Nivel"));level=Input(details.Level.ToString());editor.Children.Add(level);
    editor.Children.Add(Label("Atributos (uno por línea: nombre=valor)"));attributes=Input(string.Join(Environment.NewLine,details.Attributes.Select(x=>$"{x.Key}={x.Value}")),120);editor.Children.Add(attributes);
    editor.Children.Add(Label("Inventario (un objeto por línea)"));inventory=Input(string.Join(Environment.NewLine,details.Inventory),100);editor.Children.Add(inventory);
    editor.Children.Add(Label("Vínculos (nombre del personaje | relación, uno por línea)"));links=Input(string.Join(Environment.NewLine,details.Links),100);editor.Children.Add(links);
    editor.Children.Add(Label("Historial (una entrada por línea)"));history=Input(string.Join(Environment.NewLine,details.History),100);editor.Children.Add(history);
   }
   else editor.Children.Add(Label("Datos adicionales (JSON editable)"));
   var data=kind=="character"?null:Input(record.Data,170);if(data!=null)editor.Children.Add(data);
   var buttons=new WrapPanel();buttons.Children.Add(Action("Guardar",()=>{
    try{
    string serialized;
    if(kind=="character")
    {
     if(!int.TryParse(level!.Text,out int parsedLevel)||parsedLevel<0||parsedLevel>999)throw new Exception("Nivel no válido (0–999).");
     var attrs=new Dictionary<string,string>(StringComparer.OrdinalIgnoreCase);
     foreach(var line in attributes!.Text.Split('\n',StringSplitOptions.RemoveEmptyEntries))
     {
      int equals=line.IndexOf('=');if(equals<=0)throw new Exception("Cada atributo debe tener formato nombre=valor.");
      var key=line[..equals].Trim();if(key.Length==0)throw new Exception("El atributo necesita un nombre.");
      attrs[key]=line[(equals+1)..].Trim();
     }
     var details=new CharacterDetails{Race=race!.Text.Trim(),CharacterClass=charClass!.Text.Trim(),Level=parsedLevel,Attributes=attrs,Inventory=Lines(inventory!.Text),Links=Lines(links!.Text),History=Lines(history!.Text)};
     serialized=JsonSerializer.Serialize(details);
    }
    else {JsonDocument.Parse(data!.Text);serialized=data.Text;}
    record.Title=name.Text.Trim();if(record.Title.Length==0)throw new Exception("Introduce un nombre.");record.CampaignId=campaign.SelectedValue as string??"";record.Description=description.Text;record.System=system?.SelectedItem as string??record.System;record.Data=serialized;db.Save(record);ShowRecords(kind,title);}catch(Exception ex){MessageBox.Show(ex.Message,"No se pudo guardar");}
   }));buttons.Children.Add(Action("Eliminar",()=>{if(MessageBox.Show("¿Eliminar este registro?","Confirmar",MessageBoxButton.YesNo)==MessageBoxResult.Yes){db.Delete(record.Id);ShowRecords(kind,title);}}));editor.Children.Add(buttons);
  }
  list.SelectionChanged+=(_,_)=>Edit(list.SelectedItem as Record);Edit(null);PageHost.Content=grid;
 }
 private static List<string> Lines(string text)=>text.Split('\n',StringSplitOptions.RemoveEmptyEntries|StringSplitOptions.TrimEntries).Where(x=>x.Length>0).ToList();
 private void ShowMaps()
 {
  PageTitle.Text="Editor de mapas · cuadrícula 30 × 20";
  var root=new DockPanel();var tools=new WrapPanel{Margin=new Thickness(0,0,0,10)};DockPanel.SetDock(tools,Dock.Top);root.Children.Add(tools);
  var mapRecords=db.Get("map");var picker=new ComboBox{Width=230,ItemsSource=mapRecords,DisplayMemberPath="Title"};tools.Children.Add(picker);
  var terrain=new ComboBox{Width=160,ItemsSource=new[]{"Hierba","Bosque","Montaña","Agua","Camino","Muro","Suelo"},SelectedIndex=0};tools.Children.Add(terrain);
  var canvas=new System.Windows.Controls.Primitives.UniformGrid{Rows=20,Columns=30,Width=840,Height=560};var scroll=new ScrollViewer{Content=canvas,HorizontalScrollBarVisibility=ScrollBarVisibility.Auto,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};root.Children.Add(scroll);
  var cells=new string[600];Array.Fill(cells,"Hierba");
  string Color(string t)=>t switch{"Bosque"=>"#23533D","Montaña"=>"#6C6970","Agua"=>"#285F88","Camino"=>"#A18A65","Muro"=>"#34343B","Suelo"=>"#6C594A",_=>"#506C48"};
  void Draw(){canvas.Children.Clear();for(int i=0;i<600;i++){int index=i;var b=new Border{Width=28,Height=28,Background=(Brush)new BrushConverter().ConvertFromString(Color(cells[i]))!,BorderBrush=new SolidColorBrush(System.Windows.Media.Color.FromRgb(37,37,42)),BorderThickness=new Thickness(.5),ToolTip=$"{i%30},{i/30}: {cells[i]}"};b.MouseLeftButtonDown+=(_,_)=>{cells[index]=terrain.SelectedItem?.ToString()??"Hierba";b.Background=(Brush)new BrushConverter().ConvertFromString(Color(cells[index]))!;};canvas.Children.Add(b);}}
  void Load(Record? r){Array.Fill(cells,"Hierba");if(r!=null){try{var loaded=JsonSerializer.Deserialize<string[]>(r.Data);if(loaded?.Length==600)Array.Copy(loaded,cells,600);}catch{}}Draw();}
  picker.SelectionChanged+=(_,_)=>Load(picker.SelectedItem as Record);
  tools.Children.Add(Action("+ Nuevo mapa",()=>{var r=new Record{Kind="map",Title="Mapa "+(mapRecords.Count+1),Data=JsonSerializer.Serialize(cells)};db.Save(r);ShowMaps();}));
  tools.Children.Add(Action("Guardar mapa",()=>{if(picker.SelectedItem is Record r){r.Data=JsonSerializer.Serialize(cells);db.Save(r);MessageBox.Show("Mapa guardado.");}else MessageBox.Show("Crea o selecciona un mapa.");}));
  tools.Children.Add(Action("Exportar JSON",()=>{var dialog=new SaveFileDialog{Filter="Mapa JSON|*.json",FileName="mapa.json"};if(dialog.ShowDialog()==true)File.WriteAllText(dialog.FileName,JsonSerializer.Serialize(cells));}));
  PageHost.Content=root;if(mapRecords.Count>0)picker.SelectedIndex=0;else Draw();
 }
 private void ShowDirector()
 {
  PageTitle.Text="Mesa del Director";var panel=new StackPanel();panel.Children.Add(Label("Herramientas de sesión · dados e iniciativa"));
  var dice=new WrapPanel();var result=Label("Resultado: —");foreach(var sides in new[]{4,6,8,10,12,20,100}){int s=sides;dice.Children.Add(Action("d"+s,()=>result.Text=$"d{s}: {rng.Next(1,s+1)}"));}panel.Children.Add(dice);panel.Children.Add(result);
  panel.Children.Add(Label("Combatientes (uno por línea: nombre, iniciativa)"));var input=Input("",200);panel.Children.Add(input);
  var ordered=Input("",200);ordered.IsReadOnly=true;panel.Children.Add(Action("Ordenar iniciativa",()=>{var parsed=new List<(string Name,int Initiative)>();foreach(var line in input.Text.Split('\n')){var parts=line.Trim().Split(',');if(parts.Length==2&&int.TryParse(parts[1],out var value))parsed.Add((parts[0].Trim(),value));}ordered.Text=string.Join(Environment.NewLine,parsed.OrderByDescending(x=>x.Initiative).Select(x=>$"{x.Initiative} — {x.Name}"));}));panel.Children.Add(ordered);
  panel.Children.Add(Label("Notas de sesión (se guardan como registro en Biblioteca)"));var notes=Input("",120);panel.Children.Add(notes);panel.Children.Add(Action("Guardar notas",()=>{db.Save(new Record{Kind="library",Title="Sesión "+DateTime.Now.ToString("yyyy-MM-dd HH:mm"),Description=notes.Text});MessageBox.Show("Notas guardadas.");}));
  PageHost.Content=new ScrollViewer{Content=panel,VerticalScrollBarVisibility=ScrollBarVisibility.Auto};
 }
 private void ShowBackup()
 {
  PageTitle.Text="Copias de seguridad";var panel=new StackPanel();panel.Children.Add(Label("Exporta o importa registros mediante archivos JSON locales. Importar actualiza registros con el mismo identificador."));
  panel.Children.Add(Action("Exportar todos los registros",()=>{var dialog=new SaveFileDialog{Filter="Copia RolMaster (*.rolmaster)|*.rolmaster",FileName="RolMaster_copia.rolmaster"};if(dialog.ShowDialog()!=true)return;var kinds=new[]{"campaign","character","adventure","map","library"};var records=kinds.SelectMany(db.Get).ToList();File.WriteAllText(dialog.FileName,JsonSerializer.Serialize(records,new JsonSerializerOptions{WriteIndented=true}));MessageBox.Show($"Exportados {records.Count} registros.");}));
  panel.Children.Add(Action("Importar copia",()=>{var dialog=new OpenFileDialog{Filter="Copia RolMaster (*.rolmaster)|*.rolmaster|JSON (*.json)|*.json"};if(dialog.ShowDialog()!=true)return;try{var info=new FileInfo(dialog.FileName);if(info.Length>30_000_000)throw new InvalidDataException("El archivo supera 30 MB.");var records=JsonSerializer.Deserialize<List<Record>>(File.ReadAllText(dialog.FileName))??throw new InvalidDataException("Archivo vacío.");if(records.Count>100000||records.Any(x=>string.IsNullOrWhiteSpace(x.Id)||!new[]{"campaign","character","adventure","map","library"}.Contains(x.Kind)))throw new InvalidDataException("Formato no válido.");if(MessageBox.Show($"Importar {records.Count} registros? Los identificadores existentes se actualizarán.","Confirmar",MessageBoxButton.YesNo)==MessageBoxResult.Yes){db.Import(records);MessageBox.Show("Importación terminada.");}}catch(Exception ex){MessageBox.Show(ex.Message,"Error al importar");}}));
  panel.Children.Add(Label("Los archivos de esta primera versión contienen registros y mapas de cuadrícula. No incluyen imágenes externas ni cifrado."));PageHost.Content=panel;
 }
}
