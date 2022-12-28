using System.Security.Cryptography.X509Certificates;
using System.Text;
namespace StigLang{
    class Program{
        public static void Main(string[] args){
            string path = "";
            //Finding filepath
            foreach(string arg in args){if(arg != "" && arg != null &&File.Exists(arg) ){path = arg;break;}}
            File.WriteAllText("output.c","#include <stdlib.h>\n#include <stdio.h>\nint reg = 0;\n"+Compile(path));

        }
        public static string Compile(string path){
            FilClass something = new FilClass(path);
            string ccode = something.ccode;
            for(int i = 0; i < something.includes.Count;i++){
                ccode = Compile(something.includes[i]) + ccode;
            }
            return ccode.Replace("_();","");
        }
        
    }
    class Utils{
        public static void Error(string message){
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Error: " + message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
    class FilClass{
        List<Function> Functions = new List<Function>();
        public List<string> includes = new List<string>();
        string code;
        public string ccode;
        public FilClass(string path){
            FileInfo f = new FileInfo(path);
            ccode = "//File " + f.Name + "\n";
            code = (File.ReadAllText(path));
            //Algorythm for spliting functions
            code = code.Replace("}","{")
                       .Replace("\n","")
                       .Replace("\t","")
                       .Replace(" ", "");
            string[] codes = code.Split('{');
            //Adding all functions
            for(int i =0; i < codes.Length-1;i+=2){
                Functions.Add(new Function(codes[i],codes[i+1]));
            }
            //Starting compilation process
            for(int i = 0; i < Functions.Count;i++){
                foreach(string include in Functions[i].Includes){
                    for(int o = 0; o < includes.Count;o++){
                        if(includes[o] == include){
                            goto a;
                        }
                    }
                    includes.Add(include);
                    a:i=i;
                }
                if(Functions[i].name == "main"){
                    ccode += "int main(){";
                }else{
                    ccode += "int " + f.Name.Replace(".","_") + "_" + Functions[i].name + "(){";
                }
                foreach(object[] instruction in Functions[i].instructions){
                    if(instruction[0] == ".cusc"){
                        ccode += Functions[i].customCs[int.Parse(instruction[1].ToString())] + ";";
                    }else {
                        ccode += instruction[0] + "_" + instruction[1] + "();\n";
                    }
                }
                ccode += "\nreturn 0;\n}\n";
            }
        }
    }
    class Function{
        public string name;
        public List<object[]> instructions = new List<object[]>();
        public List<string> customCs = new List<string>();
        public List<string> Includes = new List<string>();
        public Function(string name,string code){
            this.name = name;
            string[] commands = code.Split(';');
            //Processing all commands in function
            for(int i =0; i < commands.Length-1;i++){
                string cmd = "", par = "";
                string[] comsep = commands[i].Split('.');
                cmd = comsep[0];
                for(int j = 1; j < comsep.Length;j++){
                    if(j > 1)
                        par +=".";
                    par += comsep[j];
                }
                instructions.Add(new object[2]);
                switch(cmd){
                    case "cusc":
                        customCs.Add(par);
                        instructions[^1][0] = ".cusc";
                        instructions[^1][1] = customCs.Count-1;
                        break;
                    case "incl":
                        if(par != "std"){
                            if(File.Exists(par)){
                                FileInfo f = new FileInfo(par);
                                Includes.Add(f.FullName);        
                            }else{
                                Utils.Error("File doesnt exist(Function:" +name +")");
                            }   
                        }else{
                            //TODO
                            Includes.Add("/home/jonas/Projects/Cs/StigLang/std/std"); 
                        }
                        break;
                    default:
                        instructions[^1][0] = cmd;
                        instructions[^1][1] = par;
                        break;
                }
            }
        }
    }
}