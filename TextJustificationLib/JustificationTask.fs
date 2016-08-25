namespace TextJustificationLib

open System
open System.IO
open System.Data.Sql
open System.Data.SqlTypes
open System.Data.SqlClient
open Microsoft.SqlServer.Server

module JustificationTask =

    let rec create_unjustified_list (string_list:List<string>) (num_left:int) (max_num:int) (result:List<int*string>) =
      match string_list with
      | head::tail when head.Length + 1 < num_left && tail.Length > 0 -> 
        let (row_num, current_string)::result_tail = 
          match result with
          | head::tail -> result
          | _ -> [(0, "")]
        let new_result = current_string + head + " "
        create_unjustified_list tail (num_left - head.Length - 1) max_num ((row_num+1, new_result)::result_tail) 
      | head::tail when head.Length + 1 < num_left -> 
        let (row_num, current_string)::result_tail = 
          match result with
          | head::tail -> result
          | _ -> [(0, "")]
        let new_result = current_string + head
        create_unjustified_list tail (num_left - head.Length - 1) max_num ((row_num+1, new_result)::result_tail) 
      | head::tail when head.Length + 1 >= num_left ->
        let (row_num, string)::result_tail = result
        create_unjustified_list (head::tail) max_num max_num ((row_num+1, "")::result)
      | [] -> result 
      
    let rec new_sentence_builder word_list count result =
      match word_list with
        | head::tail when count > word_list.Length && count > 0 && tail.Length > 0  ->
          new_sentence_builder tail (count-2) (String.Concat(result, head, "   "))
        | head::tail when count > word_list.Length && count > 0 && tail.Length = 0  ->
          new_sentence_builder tail (count-1) (String.Concat(result, head, " "))
        | head::tail when count = word_list.Length && count > 0 && tail.Length = 0  ->
          new_sentence_builder tail (count-1) (String.Concat(result, " ", head))
        | head::tail when count > 0 && tail.Length > 0 -> new_sentence_builder tail (count-1) (String.Concat(result, head, "  "))
        | head::tail when count = 0 && tail.Length > 0 -> new_sentence_builder tail 0 (String.Concat(result, head, " "))
        | head::tail when count = 0 && tail.Length = 0 -> new_sentence_builder tail 0 (String.Concat(result, head, ""))
        | head::tail when count = 0 -> new_sentence_builder tail 0 (String.Concat(result, head))
        | [] when count > 0 -> new_sentence_builder [] (count-1) (String.Concat(result, " "))
        | [] when count = 0 -> result + System.Environment.NewLine
    
    let rec justify_list unjustified_list (result:string) (max_num:int) =
      match unjustified_list with
      | head::tail -> 
        let row_num, (sentence:string) = head
        let remainder = max_num - sentence.TrimStart().Length
        justify_list tail (new_sentence_builder (sentence.Split(' ')|> Array.toList |> List.filter(fun x -> x.Length > 0)) remainder result) max_num
      | [] -> result
  

type SqlClrQuery =
   [<SqlProcedure()>]
   static member GetJustifiedText (text:string) (line_width:int) =
     let string_list = text.Split(' ') |> Array.toList
     let res = JustificationTask.create_unjustified_list string_list line_width line_width []
     let revList = List.rev res
     let revListFinal = revList |> List.filter(fun (x, string) -> string.Length = 40) 
     let final_string = JustificationTask.justify_list revList "" line_width
     SqlContext.Pipe.Send(final_string)

     let sqlCom = SqlCommand(String.Concat("Select '",final_string, "'"))
     SqlContext.Pipe.ExecuteAndSend sqlCom

 type SqlClrFunc =
   static member GetJustifiedTextFunc (text:string) (line_width:int) =
     let string_list = text.Split(' ') |> Array.toList
     let res = JustificationTask.create_unjustified_list string_list line_width line_width []
     let revList = List.rev res
     let revListFinal = revList |> List.filter(fun (x, string) -> string.Length = 40) 
     let final_string = JustificationTask.justify_list revList "" line_width
     match final_string.Length with
     | 0 -> ""
     | _ -> final_string
     
     
     

     

     
     
