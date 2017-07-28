(*
Copyright 2017, Sjors van Gelderen
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


// Texture coordinate data
type TexCoord = double


// Used to collect information from the file
type Accumulator =
    {
        vertices   : Vertex   List
        verIndices : Index    List
        texCoords  : TexCoord List
        texIndices : Index    List
        normals    : Index    List
    }


let accumulatorZero =
    {
        vertices   = []
        verIndices = []
        texCoords  = []
        texIndices = []
        normals    = []
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


// Reads texture coordinate data from a supplied line
let parseTexCoord = fun (acc: Accumulator) (line: string) ->
    let split = line.Split ' '
    if split.Length = 3 then
        Some { acc with texCoords = acc.texCoords @ [ double split.[1]; double split.[2] ] }
    else
        None


// Reads index data from a supplied line
let parseFace = fun (acc : Accumulator) (line: string) ->
    let split = line.Split ' '
    if split.Length = 4 then
        let folder = fun (acc: Index List * Index List * Index List) (elem: string) ->
            let contents = elem.Split '/'
            if contents.Length = 3 then
                match acc with
                | (verIndices, texIndices, normals) ->
                    (int contents.[0] :: verIndices,
                     int contents.[1] :: texIndices,
                     int contents.[2] :: normals)
            else
                show <| MalformedFace line <| ()
                acc
        
        let verIndices, texIndices, normals =
            List.fold folder ([], [], []) <| Array.toList split.[1..]
        
        Some { vertices   = acc.vertices
               verIndices = acc.verIndices @ List.rev verIndices
               texCoords  = acc.texCoords
               texIndices = acc.texIndices @ List.rev texIndices
               normals    = acc.normals    @ List.rev normals   }
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
                if line.Length > 0 then
                    let parser =
                        match line.[0] with
                        | '#' -> parseSkip
                        | 'v' -> if line.[1] = 't' then parseTexCoord else parseVertex
                        | 'f' -> parseFace
                        | _   ->
                            show <| UnrecognizedHeader (string line.[0]) <| ()
                            parseSkip
                
                    match parser acc line with
                    | Some result -> result
                    | None        -> acc
                else
                    // Empty line
                    acc
            
            let data = Seq.fold folder accumulatorZero lines
            Some { data with vertices = List.rev data.vertices }
        with
            | _ ->
                show <| FailedRead fileName <| ()
                None
    else
        show <| FileNotFound fileName <| ()
        None


let processVertices (structName: string) (model: Accumulator) =
    let verticesAmount = model.vertices.Length
        
    let verticesStart =
        sprintf "f32 %sVertices[] ATTRIBUTE_ALIGN(32) = {\n" structName
        
    let verticesFolder = fun (acc: string) (v: Vertex) ->
        acc + (sprintf "    %5.2fF, %5.2fF, %5.2fF,\n" v.x v.y v.z)
            
    let verticesString = List.fold verticesFolder verticesStart model.vertices
    verticesString.[..verticesString.Length - 3] + "\n};\n", verticesAmount


let processVerIndices (structName: string) (model: Accumulator) =
    let indicesAmount  = model.verIndices.Length

    let indicesStart =
        sprintf "u16 %sIndices[] ATTRIBUTE_ALIGN(32) = {\n" structName
        
    let indicesFolder = fun (acc: string * int) (i: Index) ->
        match acc with
        | (text, count) ->
            let indent = if count % 3 = 0 then "    " else ""
            let text'  = text + indent
            let count' = count + 1
            if count' % 3 = 0 then
                (text' + (sprintf "%5d, \n" <| i - 1), count')
            else
                (text' + (sprintf "%5d, " <| i - 1), count')

    let indicesFolded = List.fold indicesFolder (indicesStart, 0) model.verIndices
    let indicesString = match indicesFolded with (text, _) -> text
    indicesString.[..indicesString.Length - 4] + "\n};\n", indicesAmount


let processTexCoords (structName: string) (model: Accumulator) =
    let texCoordsAmount = model.texCoords.Length
        
    let texCoordsStart =
        sprintf "f32 %sTexCoords[] ATTRIBUTE_ALIGN(32) = {\n" structName
        
    let texCoordsFolder = fun (acc: string * int) (vt: TexCoord) ->
        match acc with
        | (text, count) ->
            let indent = if count % 2 = 0 then "    " else ""
            let text'  = text + indent
            let count' = count + 1
            if count' % 2 = 0 then
                (text' + (sprintf "%2.5fF, \n" <| vt), count')
            else
                (text' + (sprintf "%2.5fF, " <| vt), count')
            
    let texCoordsFolded = List.fold texCoordsFolder (texCoordsStart, 0) model.texCoords
    let texCoordsString = match texCoordsFolded with (text, _) -> text
    texCoordsString.[..texCoordsString.Length - 4] + "\n};\n", texCoordsAmount


let processTexIndices (structName: string) (model: Accumulator) =
    let texIndicesAmount  = model.texIndices.Length

    let texIndicesStart =
        sprintf "u16 %sTexIndices[] ATTRIBUTE_ALIGN(32) = {\n" structName
        
    let texIndicesFolder = fun (acc: string * int) (i: Index) ->
        match acc with
        | (text, count) ->
            let indent = if count % 3 = 0 then "    " else ""
            let text'  = text + indent
            let count' = count + 1
            if count' % 3 = 0 then
                (text' + (sprintf "%5d, \n" <| i - 1), count')
            else
                (text' + (sprintf "%5d, " <| i - 1), count')

    let texIndicesFolded = List.fold texIndicesFolder (texIndicesStart, 0) model.texIndices
    let texIndicesString = match texIndicesFolded with (text, _) -> text
    texIndicesString.[..texIndicesString.Length - 4] + "\n};\n", texIndicesAmount
    

// Writes the struct into a C file
let writeStruct = fun (structName: string) (model: Accumulator) ->
    let fileName = structName + ".obj"
    try
        let verticesString,   verticesAmount   = processVertices   structName model
        let verIndicesString, verIndicesAmount = processVerIndices structName model
        let texCoordsString,  texCoordsAmount  = processTexCoords  structName model
        let texIndicesString, texIndicesAmount = processTexIndices structName model
        
        let text =
            [
                verticesString
                verIndicesString
                texCoordsString
                texIndicesString
                sprintf "struct Model %s = {" structName
                sprintf "    .vertices         = %sVertices,"   structName
                sprintf "    .verticesAmount   = %d,"           verticesAmount
                sprintf "    .indices          = %sIndices,"    structName
                sprintf "    .indicesAmount    = %d,"           verIndicesAmount
                sprintf "    .texCoords        = %sTexCoords,"  structName
                sprintf "    .texCoordsAmount  = %d,"           texCoordsAmount
                sprintf "    .texIndices       = %sTexIndices," structName
                sprintf "    .texIndicesAmount = %d\n};"        texIndicesAmount
            ] |> String.concat "\n"
        File.WriteAllText (structName + ".c", text)
        printfn "All done!"
        
    with
        _ -> show <| FailedWrite fileName <| ()


[<EntryPoint>]
let main = fun args ->
    if argsCheck args then
        let structName = args.[0]
        match scanFile structName with
        | Some data ->
            data |> writeStruct structName
        | None      -> ()
    
    0
