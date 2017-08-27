module HCup.Router


open Microsoft.AspNetCore.Http
open System.Collections.Generic
open System
open Juraff.Tasks
open Juraff.HttpHandlers
open Juraff.FormatExpressions
open Juraff.Common

type IdHandler = int -> HttpFunc -> HttpContext -> HttpFuncResult
type IdHandlers = Dictionary<string, IdHandler>

let tryParseId stringId path (dictIdHandler: IdHandlers) next ctx =
   match Int32.TryParse(stringId) with
   | true, value -> dictIdHandler.[path] value next ctx
   | false, value -> setStatusCode 404 next ctx

let customRoutef (dictIdHandler: IdHandlers) : HttpHandler =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        let remaining = ref PathString.Empty
        match ctx.Request.Path with
        | visitPath when (visitPath.StartsWithSegments(PathString("/visits"), remaining)) ->
             tryParseId (remaining.Value.Value.Substring(1)) "/visits/%i" dictIdHandler next ctx
        | userPath when (userPath.StartsWithSegments(PathString("/users"), remaining)) -> 
            let pathString = remaining.Value.Value;
            let slashIndex = pathString.IndexOf("/visits", StringComparison.Ordinal)
            if (slashIndex > -1)
            then
                tryParseId (pathString.Substring(1,slashIndex-1)) "/users/%i/visits" dictIdHandler next ctx
            else 
                tryParseId (pathString.Substring(1)) "/users/%i" dictIdHandler next ctx
        | locationPath when (locationPath.StartsWithSegments(PathString("/locations"), remaining)) ->          
            let pathString = remaining.Value.Value
            let slashIndex = pathString.IndexOf("/avg", StringComparison.Ordinal)
            if (slashIndex > -1)
            then
                tryParseId (pathString.Substring(1,slashIndex-1)) "/locations/%i/avg" dictIdHandler next ctx
            else 
                tryParseId (pathString.Substring(1)) "/locations/%i" dictIdHandler next ctx
        | _-> shortCircuit