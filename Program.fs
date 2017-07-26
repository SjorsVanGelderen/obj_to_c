(*
Converts Wavefront .obj files to C structs
*)

open System
open System.IO


// Version of the program
let version = 1.0


// All message types
type Message =
    | HelpMessage
    | UnrecognizedHeader of string
    | MalformedVertex    of string
    | MalformedFace      of string
    | FileNotFound       of string
    //| FailedCreate       of string
    | FailedRead         of string
    | FailedWrite        of string


// Display a helpful message or an error
let show (which: Message) = fun () ->
    match which with
    | HelpMessage ->
        printfn "obj_to_c version %A\nUsage: obj_to_c structname" version
        
    | UnrecognizedHeader header ->
        printfn "Header %A not recognized" header
        
    | MalformedVertex line ->
        printfn "Malformed vertex: %A" line
        
    | MalformedFace face ->
        printfn "Malformed face: %A" face

    | FileNotFound fileName ->
        printfn "File %A not found" fileName

    (*
    | FailedCreate fileName ->
        printfn "Failed to create file %A" fileName
    *)
    
    | FailedRead fileName ->
        printfn "Failed to read file %A" fileName
        
    | FailedWrite fileName ->
        printfn "Failed to write file %A" fileName


// Vertex data
type Vertex =
    {
        x : double
        y : double
        z : double
    }


// Index data
type Index = int


// Used to collect information from the file
type Accumulator =
    {
        vertices : Vertex List
        indices  : Index List
    }


let accumulatorZero =
    {
        vertices = []
        indices  = []
    }


// Used to store the data for the eventual struct
type Model =
    {
        vertices : Vertex List
    }


let modelZero =
    {
        vertices = []
    }


// Reads vertex data from a supplied line
let parseVertex = fun (acc: Accumulator) (line: string) ->
    let split = line.Split ' '
    if split.Length = 4 then
        Some { acc with vertices = { x = double split.[1]
                                     y = double split.[2]
                                     z = double split.[3] } :: acc.vertices }
    else
        show <| MalformedVertex line <| ()
        None


// Reads index data from a supplied line
let parseIndices = fun (acc : Accumulator) (line: string) ->
    let split = line.Split ' '
    if split.Length = 5 then
        let indices = [ int split.[4]
                        int split.[3]
                        int split.[2]
                        int split.[1] ]
        Some { acc with indices = indices @ acc.indices }
    else
        show <| MalformedFace line <| ()
        None


// Checks whether the correct arguments were supplied
let argsCheck = fun args ->
    if Array.isEmpty args || Array.length args > 1 then
        show HelpMessage ()
        false
    else
        true


// Opens and reads a file, collects data from its contents
let scanFile = fun (structName: string) ->
    let fileName = structName + ".obj"
    if File.Exists fileName then
        try
            let lines = File.ReadAllLines fileName
            
            let parseSkip = fun acc _ -> Some acc
            let folder = fun (acc: Accumulator) (line: string) ->
                let parser =
                    match line.[0] with
                    | '#' -> parseSkip
                    | 'v' -> parseVertex
                    | 'f' -> parseIndices
                    | _   ->
                        show <| UnrecognizedHeader (string line.[0]) <| ()
                        parseSkip
                
                match parser acc line with
                | Some result -> result
                | None        -> acc
        
            let data = Seq.fold folder accumulatorZero lines
            Some data
        with
            | _ ->
                show <| FailedRead fileName <| ()
                None
    else
        show <| FileNotFound fileName <| ()
        None


// Generates the model data from the gathered data
let generateModel = fun (data: Accumulator) ->
    let proc = fun (acc: Vertex List) (elem: Index) ->
        data.vertices.[elem - 1] :: acc
        
    let vertices = List.fold proc [] data.indices
    { vertices = vertices }


// Writes the struct into a C file
let writeStruct = fun (structName: string) (model: Model) ->
    let fileName = structName + ".obj"
    try
        let size = model.vertices.Length
        let start = sprintf "f32 %s[%d] = {\n" structName size
        
        let folder = fun (acc: string) (v: Vertex) ->
            acc + (sprintf "    %5.2fF, %5.2fF, %5.2fF,\n" v.x v.y v.z)

        let text = (List.fold folder start model.vertices) + "};"
        File.WriteAllText (structName + ".c", text)
        
    with
        _ -> show <| FailedWrite fileName <| ()


[<EntryPoint>]
let main = fun args ->
    if argsCheck args then
        let structName = args.[0]
        match scanFile structName with
        | Some data -> generateModel data |> writeStruct structName
        | None      -> ()
    
    0
