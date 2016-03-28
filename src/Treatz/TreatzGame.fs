﻿namespace TreatzGame

open CommonData
open SDLUtility
open SDLGeometry
open SDLKeyboard
open SDLGameController


[<Struct>]
[<CustomEquality>]
[<CustomComparison>]
// used in the quadtrees and pathfinding algorithms 
type FastPoint(x: int, y: int) =
    member __.X = x
    member __.Y = y
    
    override lhs.Equals rhs =
        let rhs = (rhs :?> FastPoint)
        lhs.X = lhs.X && lhs.Y = rhs.X
    
    interface System.IComparable with
        member lhs.CompareTo rhs =
            let rhs = (rhs :?> FastPoint)
            let c = compare lhs.X rhs.X
            if c <> 0 then c else
            compare lhs.Y rhs.Y
    
    override p.GetHashCode() =
        x + 65536 * y

    member __.GridX = x / cellWidth
    member __.GridY = y / cellHeight
    

type PlayerData = 
    {dragonsCaught : int}
    with static member Blank = {dragonsCaught = 0}

type DragonData =
    | Nothing
    | Roam of roamingFrames : int 
    | Seek of (double * double) list
    | Temporary of treat : double * double // no really, this is going


type MikishidaKinds =
    | Player of PlayerData
    | Dragon of DragonData
    | Treat
    | Mountainountain
    | AntiDragonFoam 
    | Squirrel
    | Cat
    | Otter
    with 
    member this.defaultSpeed =
        match this with
        | Player _ -> 0.5
        | Dragon _  -> 1.0
        | _ -> 0.9

type Mikishida = 
    { kind : MikishidaKinds; location : double * double; velocity : double * double }
    with 
    member this.Size =
        match this.kind with
        | Treat -> 5<px>, 5<px>
        | _ -> 10<px>, 10<px>

    member this.AsRect = 
        let w, h = this.Size
        { 
          X = (fst this.location |> int)*1<px> 
          Y = (snd this.location |> int)*1<px>
          Width = w
          Height = h 
        }
    member this.AsQuadBounds : QuadTree.QuadBounds = 
        let w, h = this.Size
        { 
          x = ((fst this.location) ) |> int
          y = ((snd this.location) ) |> int
          width = int w 
          height = int h
        }
    member this.Distance(other:Mikishida) =
        let xd = fst other.location - fst this.location
        let yd = snd other.location - snd this.location
        sqrt(xd*xd+yd*yd)

    member this.ManhattanDistance(other:Mikishida) =
        abs(fst other.location - fst this.location) + abs(snd other.location - snd this.location)


type TreatzState =
    { Player1 : Mikishida
      Player2 : Mikishida
      Mikishidas : Mikishida list
//      StaticLookup : Set<double*double> // set of immovable Juans locations for fast lookup
      PressedKeys : Set<ScanCode> 
      Controllers : Set<ControllerButton> * Set<ControllerButton>
      Sprites : Map<string, SDLTexture.Texture>
      TurkeyAngle : float
      Chaos : System.Random
      }
    with member this.findMikishidas pred bounds =
            this.Mikishidas |> List.filter(fun m -> pred m && overlapq(m.AsQuadBounds, bounds))
