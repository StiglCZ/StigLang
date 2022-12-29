namespace StigLang{
    class Program{
        public static void Main(string[] args){
            string path = "";
            //Finding filepath
            foreach(string arg in args){if(arg != "" && arg != null &&File.Exists(arg) ){path = arg;break;}}
            if(path == ""){
                Utils.Error("Error: No input files");
            }
            if(!File.Exists(path)){
                Utils.Error("File "+  path + " doesnt exist");
            }
            File.WriteAllText("output.c","#include <stdlib.h>\n#include <stdio.h>\nint r1egs[255];int pos = 0;"+Compile(path));
            Console.WriteLine("Succesfully compiled at " + DateTime.Now);
        }

        public static string Compile(string path){
            FilClass something = new FilClass(path);
            string ccode = something.ccode;
            for(int i = 0; i < something.includes.Count;i++){
                ccode = Compile(something.includes[i]) + ccode;
            }
            return ccode.Replace("_();","").Replace("reg","r1egs[pos]");
        }
        
    }
    class Utils{
        public static void Error(string message){
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.WriteLine("Error: " + message);
            Console.ForegroundColor = ConsoleColor.White;
            Environment.Exit(-1);
        }
        public static void Warning(string message){
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine("Warning: " + message);
            Console.ForegroundColor = ConsoleColor.White;
        }
    }
    class FilClass{
        List<Function> Functions = new List<Function>();
        public List<string> includes = new List<string>();
        string code;
        public string ccode;
        public FilClass(string path){
            //Getting filename
            FileInfo f = new FileInfo(path);
            ccode = "";
            code = (File.ReadAllText(path));
            //Algorythm for spliting functions
            code = code.Replace("}","{")
                       .Replace("\n","")
                       .Replace("\t","")
                       .Replace(" ", "");
            string[] codes = code.Split('{');
            if(codes.Length < 2){
                Utils.Warning("No functions found in file: " + f.Name);
            }
            //Adding all functions
            for(int i =0; i < codes.Length-1;i+=2){
                Functions.Add(new Function(codes[i],codes[i+1]));
            }
            //Starting compilation process
            for(int i = 0; i < Functions.Count;i++){
                //Finding all duplicate includes + adding them
                foreach(string include in Functions[i].Includes){
                    for(int o = 0; o < includes.Count;o++){
                        if(includes[o] == include){
                            goto a;
                        }
                    }
                    includes.Add(include);
                    a:i=i;
                }
                //Getting name for all functions
                if(Functions[i].name == "main"){
                    ccode += "int main(){";
                }else{
                    ccode = "int " + f.Name.Replace(".","_") + "_" + Functions[i].name + "();"+ccode;
                    ccode += "int " + f.Name.Replace(".","_") + "_" + Functions[i].name + "(){";
                }
                //Getting each instruction c code
                foreach(object[] instruction in Functions[i].instructions){
                    //Custom c
                    
                    if(instruction[0] == ".cusc"){
                        ccode += Functions[i].customCs[int.Parse(instruction[1].ToString())] + ";";
                    //If statment
                    }else if(instruction[0] == ".if"){
                        ccode += "if(reg == " +instruction[1].ToString() +"  ) "+instruction[2].ToString().Replace(".","_")+"();";
                    }else if(instruction[0] == ".drop"){
                        ccode += "r1egs[pos] = 0;";
                        ccode += "if(pos > 0) pos--;";
                    }else if(instruction[0] == ".move"){
                        ccode += "pos++;";
                    }else {
                        ccode += instruction[0] + "_" + instruction[1] + "();";
                    }
                }
                //Closing the commands
                ccode += "return 0;}";
                
            }
            ccode = "//File " + f.Name + "\n" + ccode;
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
            //Parsing all commands in function
            for(int i =0; i < commands.Length-1;i++){
                //Spliting command to two parts (example hello1.hello2.hello3) cmd = hello1 par = hello2.hello3
                string cmd = "", par = "";
                string[] comsep = commands[i].Split('.');
                cmd = comsep[0];
                for(int j = 1; j < comsep.Length;j++){
                    if(j > 1)
                        par +=".";
                    par += comsep[j];
                }
                instructions.Add(new object[3]);
                //Parsing each cmd with par
                switch(cmd){
                    //Custom c case
                    case "cusc":
                        //Adding to temporary storage(customCs)
                        customCs.Add(par);
                        //Giving instructions how to get to it(. in instruction[x][0] is imposible to do normally)
                        instructions[^1][0] = ".cusc";
                        instructions[^1][1] = customCs.Count-1;
                        break;
                        //NOT DONE YET DONT USE
                    case "if":
                        string[] spl = par.Split('.');
                        instructions[^1][0] = ".if";
                        instructions[^1][1] = spl[0];
                        instructions[^1][2] = spl[1];
                        break;
                    //Including functions
                    case "incl":
                            if(File.Exists(par)){
                                FileInfo f = new FileInfo(par);
                                Includes.Add(f.FullName);        
                            }else{
                                Utils.Error("File doesnt exist(Function:" +name +")");
                            }   
                        break;
                    case "drop":
                        instructions[^1][0] = ".drop";
                        break;
                    case "move":
                        instructions[^1][0] = ".move";
                        break;
                    default:
                    //Normally registering func call
                        instructions[^1][0] = cmd;
                        instructions[^1][1] = par;
                        break;
                }
            }
        }
    }
}