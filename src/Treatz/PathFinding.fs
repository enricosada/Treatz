﻿module PathFinding

  open System.Collections.Generic
  open System.Linq
  open TreatzGame

  let getNeighbours allNodes node=            
      let x = node.Identity.X
      let y = node.Identity.Y
      let identites = [(1,0); (-1, 0); (0, -1); (0, 1)]
                      |> List.map(fun (o,r) -> 
                          {X= (x + o); Y= (y + r)} )
      [|for i in identites do
        if Map.containsKey (i.X, i.Y) allNodes then
            yield allNodes.[(i.X, i.Y)] |]

  let createInitialFrontier state : HashSet<Node> =
    let q = HashSet<Node>()
    q.Add(state) |> ignore
    q

  let expandNode (node:Node) = 
    node.Neighbours 

  // not the most efficient way to calc distance but ...
  let calcDistance (node:Node) goalNode =    
    let x = double (node.Identity.X - goalNode.Identity.X)
    let y = double( node.Identity.Y - goalNode.Identity.Y) 
    sqrt( x * x + y * y) |> int

  let calculatePlayerBasedCost baseNode (players: Mikishida list) =
    let calc baseV p =  
      let x = double (baseV.X - p.X)
      let y = double( baseV.Y - p.Y) 
      sqrt( x * x + y * y) |> int
    baseNode.Cost + (players 
                      |> List.map(fun x -> calc baseNode.Identity {NodeVector.X = x.location.GridX; Y= x.location.GridY}) 
                      |> List.sum )
  
  let search startNode goal (playersNodes: Mikishida list): Node array =
    let frontier = createInitialFrontier startNode 
    let explored = new HashSet<Node>() //mutable
    if (frontier.Count = 0) then 
        [||]
    else  
      while frontier.Count > 0 do      
        let currentNode = 
            frontier.ToArray() 
            |> Seq.minBy(fun x -> 1 + calcDistance x goal + (calculatePlayerBasedCost x playersNodes))
        frontier.Remove(currentNode)  |> ignore
        if (currentNode.Identity <> goal.Identity) then
          explored.Add(currentNode) |> ignore 
        
          (expandNode currentNode)
          |> Seq.iter(fun n -> 
                if not( explored.Contains(n)) then frontier.Add(n) |> ignore )
          
        else
          frontier.Clear()  //yuck
          //explored.ToArray()
          

      explored.ToArray()

