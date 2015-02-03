﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO.Ports;
using System.Threading;

using System.IO;

namespace _2D_array_test
{
   
    class Program
    {
        #region Path_Planning_Part

        public int abs(int value)
        {

            if (value >= 0) { return value; }
            else { return value * (-1); }
        }

        public static char[,] grid = new char[24, 40];

        public static string[] grid_1 = new string[24];

        public static int[,] result_grid = new int[24, 40];

        public static int[,] temp_grid = new int[24, 40];

        public static int i = 0;
        public static int j = 0;
        public static int k = 0;

        public static char[,] unit_100th = new char[24, 40];
        public static char[,] unit_10th = new char[24, 40];
        public static char[,] unit_1st = new char[24, 40];

        public class coordinate
        {

            public int x;
            public int y;
        };

        public class vehicle
        {

            public int size;

            public int current_perspective = 0;

            public coordinate outer_1 = new coordinate();
            public coordinate outer_2 = new coordinate();
            public coordinate outer_3 = new coordinate();
            public coordinate outer_4 = new coordinate();

            ///////////////////////////////////////////
            public int path_num = 0;

            public vehicle[] children = new vehicle[8];

            //////////////////////////////////////////
            public int enqueue = 0;
            public int dequeue = 0;

            public int visited = 0;

            public int weight = 0;

            public vehicle() { }

            public vehicle(int size_, int perspective, int x, int y, int num)
            {

                size = size_;

                current_perspective = perspective;

                outer_1.x = x; 
                outer_1.y = y;

                outer_2.x = x + size_ - 1; 
                outer_2.y = y;

                outer_3.x = x; 
                outer_3.y = y + size_ - 1;

                outer_4.x = x + size_ - 1; 
                outer_4.y = y + size_ - 1;

                path_num = num;

                children[0] = null; children[1] = null; children[2] = null; children[3] = null;
                children[4] = null; children[5] = null; children[6] = null; children[7] = null;

                enqueue = 0;
                dequeue = 0;

                visited = 0;

                weight = 0;
            }
        };

        //***** Path Planning ******//

        public static vehicle vehicle_1 = new vehicle();

        public static vehicle[,] node = new vehicle[24, 40];

        public static vehicle[] q = new vehicle[24*40];

        public static int row = 24;
        public static int column = 40;

        public static int size = 0;

        ////////////////////////////////


        public static int q_size = 24 * 40 - 1;
        public static int q_level = -1;
        ////////////////////////////////

        public static int count = 0;
        //*************************//  
        public static bool right_move_check(vehicle vehicle_1)
        {

	        int i = 0;
	        int count = 0;
	        int size = vehicle_1.size;

	        for(i=0; i<size; i++){

                try
                {
                    if (grid[vehicle_1.outer_2.y + i, vehicle_1.outer_2.x + 1] == '0') { count++; }
                }
                catch(IndexOutOfRangeException){}
	        }

	        if(count == size){

		        return true;
	        }
	        else{ return false; }
        }

        public static vehicle right_movement(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        vehicle_1.outer_2.x = vehicle_1.outer_2.x + 1;
	        vehicle_1.outer_3.x = vehicle_1.outer_3.x + 1;
	        vehicle_1.outer_1.x = vehicle_1.outer_1.x + 1;
	        vehicle_1.outer_4.x = vehicle_1.outer_4.x + 1;

	        return vehicle_1;
        }
        /////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////

        public static bool left_move_check(vehicle vehicle_1)
        {

	        int i = 0;
	        int count = 0;
	        int size = vehicle_1.size;

	        for(i=0; i<size; i++){
                try
                {
                    if (grid[vehicle_1.outer_1.y + i, vehicle_1.outer_1.x - 1] == '0') { count++; }
                }
                catch(IndexOutOfRangeException){}
	        }

	        if(count == size){

		        return true;
	        }
	        else{ return false; }
        }

        public static vehicle left_movement(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        vehicle_1.outer_1.x = vehicle_1.outer_1.x - 1;
	        vehicle_1.outer_2.x = vehicle_1.outer_2.x - 1;
	        vehicle_1.outer_3.x = vehicle_1.outer_3.x - 1;
	        vehicle_1.outer_4.x = vehicle_1.outer_4.x - 1;

	        return vehicle_1;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////

        public static bool up_move_check(vehicle vehicle_1)
        {

	        int i = 0;
	        int count = 0;
	        int size = vehicle_1.size;

	        for(i=0; i<size; i++){

                try
                {
                    if (grid[vehicle_1.outer_1.y - 1, vehicle_1.outer_1.x + i] == '0') { count++; }
                }
                catch(IndexOutOfRangeException){}
	        }

	        if(count == size){

		        return true;
	        }
	        else{ return false; }
        }

        public static vehicle up_movement(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        vehicle_1.outer_1.y = vehicle_1.outer_1.y - 1;
	        vehicle_1.outer_2.y = vehicle_1.outer_2.y - 1;
	        vehicle_1.outer_3.y = vehicle_1.outer_3.y - 1;
	        vehicle_1.outer_4.y = vehicle_1.outer_4.y - 1;

	        return vehicle_1;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////

        public static bool down_move_check(vehicle vehicle_1)
        {

	        int i = 0;
	        int count = 0;
	        int size = vehicle_1.size;

	        for(i=0; i<size; i++){
                try
                {
                    if (grid[vehicle_1.outer_3.y + 1, vehicle_1.outer_3.x + i] == '0') { count++; }
                }
                catch(IndexOutOfRangeException){}
	        }

	        if(count == size){

		        return true;
	        }
	        else{ return false; }
        }

        public static vehicle down_movement(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        vehicle_1.outer_1.y = vehicle_1.outer_1.y + 1;
	        vehicle_1.outer_2.y = vehicle_1.outer_2.y + 1;
	        vehicle_1.outer_3.y = vehicle_1.outer_3.y + 1;
	        vehicle_1.outer_4.y = vehicle_1.outer_4.y + 1;

	        return vehicle_1;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////

        public static bool diagonal_right_up_check(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        if(vehicle_1.outer_2.x + 1 == 39 || vehicle_1.outer_2.y == 23){ return false; }

            if (right_move_check(vehicle_1) && up_move_check(vehicle_1) && (grid[vehicle_1.outer_2.y - 1, vehicle_1.outer_2.x + 1] == '0'))
            {
                return true;
	        }
	        else{ return false; }

            return false;
        }

        public static vehicle move_diagonal_right_up(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        vehicle_1.outer_1.x = vehicle_1.outer_1.x + 1;
	        vehicle_1.outer_1.y = vehicle_1.outer_1.y - 1;

	        vehicle_1.outer_2.x = vehicle_1.outer_2.x + 1;
	        vehicle_1.outer_2.y = vehicle_1.outer_2.y - 1;

	        vehicle_1.outer_3.x = vehicle_1.outer_3.x + 1;
	        vehicle_1.outer_3.y = vehicle_1.outer_3.y - 1;

	        vehicle_1.outer_4.x = vehicle_1.outer_4.x + 1;
	        vehicle_1.outer_4.y = vehicle_1.outer_4.y - 1;

	        return vehicle_1;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////

        public static bool diagonal_right_down_check(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        if(vehicle_1.outer_2.x + 1 == 39 || vehicle_1.outer_2.y == 23){ return false; }

	        if(right_move_check(vehicle_1) && down_move_check(vehicle_1) && (grid[vehicle_1.outer_4.y + 1, vehicle_1.outer_4.x + 1] == '0')){

                return true;
	        }
	        else{ return false; }

            return false;
        }

        public static vehicle move_diagonal_right_down(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        vehicle_1.outer_1.x = vehicle_1.outer_1.x + 1;
	        vehicle_1.outer_1.y = vehicle_1.outer_1.y + 1;

	        vehicle_1.outer_2.x = vehicle_1.outer_2.x + 1;
	        vehicle_1.outer_2.y = vehicle_1.outer_2.y + 1;

	        vehicle_1.outer_3.x = vehicle_1.outer_3.x + 1;
	        vehicle_1.outer_3.y = vehicle_1.outer_3.y + 1;

	        vehicle_1.outer_4.x = vehicle_1.outer_4.x + 1;
	        vehicle_1.outer_4.y = vehicle_1.outer_4.y + 1;

	        return vehicle_1;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////

        public static bool diagonal_left_up_check(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        if(vehicle_1.outer_2.x + 1 == 39 || vehicle_1.outer_2.y == 23){ return false; }

            if (left_move_check(vehicle_1) && up_move_check(vehicle_1) && (grid[vehicle_1.outer_1.y - 1, vehicle_1.outer_1.x - 1] == '0'))
            {
                return true;
	        }
	        else{ return false; }

            return false;
        }

        public static vehicle move_diagonal_left_up(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        vehicle_1.outer_1.x = vehicle_1.outer_1.x - 1;
	        vehicle_1.outer_1.y = vehicle_1.outer_1.y - 1;

	        vehicle_1.outer_2.x = vehicle_1.outer_2.x - 1;
	        vehicle_1.outer_2.y = vehicle_1.outer_2.y - 1;

	        vehicle_1.outer_3.x = vehicle_1.outer_3.x - 1;
	        vehicle_1.outer_3.y = vehicle_1.outer_3.y - 1;

	        vehicle_1.outer_4.x = vehicle_1.outer_4.x - 1;
	        vehicle_1.outer_4.y = vehicle_1.outer_4.y - 1;

	        return vehicle_1;
        }

        /////////////////////////////////////////////////////////////////////////////////////////
        /////////////////////////////////////////////////////////////////////////////////////////

        public static bool diagonal_left_down_check(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        if(vehicle_1.outer_2.x + 1 == 39 || vehicle_1.outer_2.y == 23){ return false; }

	        if(left_move_check(vehicle_1) && down_move_check(vehicle_1) && (grid[vehicle_1.outer_3.y + 1, vehicle_1.outer_3.x - 1] == '0')){
            
                return true;
	        }
	        else{ return false; }

            return false;
        }

        public static vehicle move_diagonal_left_down(vehicle vehicle_1)
        {

	        int size = vehicle_1.size;

	        vehicle_1.outer_1.x = vehicle_1.outer_1.x - 1;
	        vehicle_1.outer_1.y = vehicle_1.outer_1.y + 1;

	        vehicle_1.outer_2.x = vehicle_1.outer_2.x - 1;
	        vehicle_1.outer_2.y = vehicle_1.outer_2.y + 1;

	        vehicle_1.outer_3.x = vehicle_1.outer_3.x - 1;
	        vehicle_1.outer_3.y = vehicle_1.outer_3.y + 1;

	        vehicle_1.outer_4.x = vehicle_1.outer_4.x - 1;
	        vehicle_1.outer_4.y = vehicle_1.outer_4.y + 1;

	        return vehicle_1;
        }

        public static vehicle[,] node_init(vehicle[,] in_node)
        {

            int i = 0;
            int j = 0;

            for (i = 0; i < row; i++)
            {

                for (j = 0; j < column; j++)
                {

                    in_node[i, j] = new vehicle(size, 0, j, i, 0);
                }
            }

            return in_node;
        }

        public static void graph_reconstruct(vehicle[,] in_node, int target_x, int target_y, int start_x, int start_y)
        {

            int i = 0;
            int j = 0;

            vehicle[,] temp_node = in_node;

            for (i = 0; i < 24; i++)
            {

                for (j = 0; j < 40; j++)
                {

                    if (grid[i, j] == '0')
                    {
                        in_node[i, j].visited = 0;

                        if (up_move_check(in_node[i, j]))
                        {
                            in_node[i, j].children[0] = new vehicle();
                            in_node[i, j].children[0] = in_node[i - 1, j];
                        }
                        if (diagonal_right_up_check(in_node[i, j]))
                        {
                            in_node[i, j].children[1] = new vehicle();
                            in_node[i, j].children[1] = in_node[i - 1, j + 1];
                        }
                        if (right_move_check(in_node[i, j]))
                        {
                            in_node[i, j].children[2] = new vehicle();
                            in_node[i, j].children[2] = in_node[i, j + 1];
                        }
                        if (diagonal_right_down_check(in_node[i, j]))
                        {
                            in_node[i, j].children[3] = new vehicle();
                            in_node[i, j].children[3] = in_node[i + 1, j + 1];
                        }
                        if (down_move_check(in_node[i, j]))
                        {
                            in_node[i, j].children[4] = new vehicle();
                            in_node[i, j].children[4] = in_node[i + 1, j];
                        }
                        if (diagonal_left_down_check(in_node[i, j]))
                        {
                            in_node[i, j].children[5] = new vehicle();
                            in_node[i, j].children[5] = in_node[i + 1, j - 1];
                        }
                        if (left_move_check(in_node[i, j]))
                        {
                            in_node[i, j].children[6] = new vehicle();
                            in_node[i, j].children[6] = in_node[i, j - 1];
                        }
                        if (diagonal_left_up_check(in_node[i, j]))
                        {
                            in_node[i, j].children[7] = new vehicle();
                            in_node[i, j].children[7] = in_node[i - 1, j - 1];
                        }
                    }
                    else
                    {

                        in_node[i, j].visited = 4;
                    }
                }
            }
        }

        public static void find_path_BFS(vehicle node_1, int target_x, int target_y)
        {

	        int i = 0;
	        int j = 0;
	        int l = 0;

	        vehicle root;
	        vehicle pop;

	        i=0; j=0;

	        root = node[node_1.outer_1.y, node_1.outer_1.x];

	        q_level++;		// q_level 0에서 시작 / -1 : q가 비워짐
	        q[q_level] = root;

	        while(q_level != -1){

		        l++;

		        pop = q[0];
		
		        node[q[0].outer_1.y, q[0].outer_1.x].visited = 1;
		        q_level--;
		        for(i=0; i<=q_level; i++){
			
			        q[i] = q[i+1];
		        }

		        if((pop.outer_1.x == target_x) && (pop.outer_1.y == target_y)){

                    Console.WriteLine("x : {0} y : {1}", pop.outer_1.x, pop.outer_1.y);
                    Console.WriteLine("Vehicle has arrived at the destination");
			        break;
		        }
		        for(i=0; i<8; i++){
			
			        if( (pop.children[i] != null) && ( node[(pop.children[i]).outer_1.y, (pop.children[i]).outer_1.x].visited == 0) ){

				        node[(pop.children[i]).outer_1.y, (pop.children[i]).outer_1.x].weight = node[pop.outer_1.y, pop.outer_1.x].weight + 1;

				        q_level++;

				        q[q_level] = node[pop.outer_1.y, pop.outer_1.x].children[i];
				
				        node[q[q_level].outer_1.y, q[q_level].outer_1.x].visited = 1;
			        }
		        }
	        }

	        for(i=0; i<row; i++){
	
		        for(j=0; j<column; j++){
					
			        result_grid[i, j] = node[i, j].weight;
				
		        }
	        }
        }

        

        #endregion

        static void Main(string[] args)
        {
            string write_data;
            string read_data;

            string input_grid;

            SerialPort serialport = new SerialPort();

            Console.WriteLine("--------- Grid Input -------------");

            for (i = 0; i < 24; i++)
            {
                input_grid = Console.ReadLine();

                for (j = 0; j < 40; j++)
                {
                    grid[i, j] = input_grid[j*2];
                }
            }

            Console.WriteLine("--------- Grid Display -------------");

            for (i = 0; i < 24; i++)
            {
                for (j = 0; j < 40; j++)
                {
                    Console.Write("{0} ", grid[i,j] );
                }

                Console.WriteLine(" ");
            }

            serialport.PortName = "COM6";   // Vehicle 1 : COM6
                                            // Vehicle 2 : COM12
            serialport.BaudRate = 9600;
            serialport.DataBits = 8;
            serialport.StopBits = StopBits.One;
            serialport.ReadTimeout = 200;
            serialport.WriteTimeout = 200;

            serialport.Open();

            if (serialport.IsOpen)
            {

                Console.WriteLine("Communication has been established. \n");
            }

            while (serialport.IsOpen)
            {
                write_data = (Console.ReadKey().KeyChar).ToString();
                Console.WriteLine();

                serialport.WriteLine((write_data[0]).ToString());
                
                if (write_data == "a")
                {

                    try
                    {
                        read_data = serialport.ReadTo("\r");
                        Console.WriteLine(read_data);
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(serialport.ReadExisting());
                    }
                }
                else if (write_data == "b")
                {
                    try
                    {
                        read_data = serialport.ReadTo("\r");
                        Console.WriteLine(read_data);
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(serialport.ReadExisting());
                    }
                }
                else if (write_data == "c")
                {
                    try
                    {
                        read_data = serialport.ReadTo("\r");
                        Console.WriteLine(read_data);
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(serialport.ReadExisting());
                    }
                }

                if(write_data == "f")
                {
                    try
                    {

                        #region Start_Destination_Size Declaration

                        string starting_point_x;
                        string starting_point_y;
                        string destination_x;
                        string destination_y;
                        string size;

                        int start_x = 0; ;
                        int start_y = 0; ;
                        int dest_x = 0; ;
                        int dest_y = 0; ;
                        int size_ = 0; ;

                        char[] start_x_ = new char[3] { '0', '0', '0' };
                        char[] start_y_ = new char[3] { '0', '0', '0' };
                        char[] dest_x_ = new char[3] { '0', '0', '0' };
                        char[] dest_y_ = new char[3] { '0', '0', '0' };
                        char[] s_ = new char[3] { '0', '0', '0' };

                        Console.Write("starting_point.x : ");
                        starting_point_x = Console.ReadLine();

                        Console.Write("starting_point.y : ");
                        starting_point_y = Console.ReadLine();

                        Console.Write("destination.x : ");
                        destination_x = Console.ReadLine();

                        Console.Write("destination.y : ");
                        destination_y = Console.ReadLine();
                        
                        Console.Write("size : ");
                        size = Console.ReadLine();

                        start_x = Int32.Parse(starting_point_x);
                        start_y = Int32.Parse(starting_point_y);

                        dest_x = Int32.Parse(destination_x);
                        dest_y = Int32.Parse(destination_y);

                        size_ = Int32.Parse(size);

                        start_x_[2] = (char)((start_x / 100) + 48);         // Hundredth
                        start_x_[1] = (char)(((start_x / 10)%10) + 48);     // Tenth
                        start_x_[0] = (char)((start_x % 10) + 48);          // First

                        start_y_[2] = (char)((start_y / 100) + 48);
                        start_y_[1] = (char)(((start_y / 10) % 10) + 48);
                        start_y_[0] = (char)((start_y % 10) + 48);

                        dest_x_[2] = (char)((dest_x / 100) + 48);
                        dest_x_[1] = (char)(((dest_x / 10) % 10) + 48);
                        dest_x_[0] = (char)((dest_x % 10) + 48);

                        dest_y_[2] = (char)((dest_y / 100) + 48);
                        dest_y_[1] = (char)(((dest_y / 10) % 10) + 48);
                        dest_y_[0] = (char)((dest_y % 10) + 48);

                        s_[2] = (char)((size_ / 100) + 48);
                        s_[1] = (char)(((size_ / 10) % 10) + 48);
                        s_[0] = (char)((size_ % 10) + 48);

                        serialport.WriteLine(start_x_[2].ToString());
                        serialport.WriteLine(start_x_[1].ToString());
                        serialport.WriteLine(start_x_[0].ToString());

                        serialport.WriteLine(start_y_[2].ToString());
                        serialport.WriteLine(start_y_[1].ToString());
                        serialport.WriteLine(start_y_[0].ToString());

                        serialport.WriteLine(dest_x_[2].ToString());
                        serialport.WriteLine(dest_x_[1].ToString());
                        serialport.WriteLine(dest_x_[0].ToString());

                        serialport.WriteLine(dest_y_[2].ToString());
                        serialport.WriteLine(dest_y_[1].ToString());
                        serialport.WriteLine(dest_y_[0].ToString());

                        serialport.WriteLine(s_[2].ToString());
                        serialport.WriteLine(s_[1].ToString());
                        serialport.WriteLine(s_[0].ToString());

                        #endregion

                        #region Graph_Node_Initialization
                        ///////////////////////////////////////////////////////////
                        vehicle_1.size = size_;

                        vehicle_1.outer_1.x = start_x;
                        vehicle_1.outer_1.y = start_y;

                        vehicle_1.outer_2.x = start_x + size_ - 1;
                        vehicle_1.outer_2.y = start_y;

                        vehicle_1.outer_3.x = start_x;
                        vehicle_1.outer_3.y = start_y + size_ - 1;

                        vehicle_1.outer_4.x = start_x + size_ - 1;
                        vehicle_1.outer_4.y = start_y + size_ - 1;

                        vehicle_1.visited = 1;

                        vehicle_1.weight = 0;
                        //////////////////////////////////////////////////////////
                        #endregion

                        #region Node_Initialization

                        for (i = 0; i < row; i++)
                        {

                            for (j = 0; j < column; j++)
                            {

                                node[i, j] = new vehicle(size_, 0, j, i, 0);
                                
                                result_grid[i,j] = temp_grid[i,j];
                            }
                        }
                        #endregion

                        #region Graph_Construction

                        graph_reconstruct(node, dest_x, dest_y, start_x, start_y);

                        
                        for(i=0; i<24; i++){
                            for(j=0; j<40; j++){

                                //Console.WriteLine("Node[{0}][{1}]", i, j);
                                for(k=0; k<8; k++){

                                    if(node[i,j].children[k] != null){

                                        //Console.WriteLine("Child[{0}] : {1} {2}", k, node[i, j].children[k].outer_1.x, node[i, j].children[k].outer_1.y);
                                    }
                                }
                            }
                        }
                        
                        Console.WriteLine("Graph Reconstruction Complete");
                        #endregion

                        #region BFS Path Planning

                        find_path_BFS(vehicle_1, dest_x, dest_y);

                        for (i = 0; i < 24; i++)
                        {
                            for (j = 0; j < 40; j++)
                            {
                                Console.Write("{0,3} ",result_grid[i,j].ToString());
                            }
                            Console.WriteLine(" ");
                        }

                        #endregion

                        string path = @"C:\Users\Win8\ERICA\SSM Project\SSM Project MVCC\C# Test\2D_Path_Planning_Following_1.0\Test_Weight_Grid.txt";

                        System.IO.File.WriteAllText(path, string.Empty);

                            for (i = 0; i < 24; i++)
                            {
                                for (j = 0; j < 40; j++)
                                {
                                    using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
                                    {
                                        file.Write(result_grid[i,j].ToString());
                                        file.Write(" ");
                                    }

                                    //System.IO.File.AppendAllText(path, result_grid[i, j].ToString());
                                    //System.IO.File.WriteAllText(path, result_grid[i,j].ToString());
                                }
                                using (System.IO.StreamWriter file = new System.IO.StreamWriter(path, true))
                                {
                                    file.WriteLine(" ");
                                }
                            }

                        #region Weight_Calculation
                        for (i=0; i<row; i++){
                            for(j=0; j<column; j++){

                                if (result_grid[i, j] < 10)
                                {
                                    unit_100th[i, j] = '0';
                                    unit_10th[i, j] = '0';
                                    unit_1st[i, j] = result_grid[i, j].ToString()[0];
                                }
                                else if (result_grid[i, j] < 100)
                                {
                                    unit_100th[i, j] = '0';
                                    unit_10th[i, j] = result_grid[i, j].ToString()[0];
                                    unit_1st[i, j] = result_grid[i, j].ToString()[1];
                                }
                                else if (result_grid[i, j] < 1000)
                                {
                                    unit_100th[i, j] = result_grid[i, j].ToString()[0];
                                    unit_10th[i, j] = result_grid[i, j].ToString()[1];
                                    unit_1st[i, j] = result_grid[i, j].ToString()[2];
                                }
                                //Console.WriteLine("{0} {1} : {2} {3} {4}", i, j, unit_100th[i, j], unit_10th[i, j], unit_1st[i, j]);
                            }
                        }
                        #endregion

                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(serialport.ReadExisting());
                    }
                }

                else if (write_data == "g")
                {
                    try
                    {
                        #region Transmit_Weight_Grid
                        for (i = 0; i < row; i++)
                        {
                            for (j = 0; j < column; j++)
                            {
                                serialport.WriteLine(unit_100th[i, j].ToString());
                            }
                        }
                        for (i = 0; i < row; i++)
                        {
                            for (j = 0; j < column; j++)
                            {
                                serialport.WriteLine(unit_10th[i, j].ToString());
                            }
                        }
                        for (i = 0; i < row; i++)
                        {
                            for (j = 0; j < column; j++)
                            {
                                serialport.WriteLine(unit_1st[i, j].ToString());
                            }
                        }
                        Console.WriteLine("TX Complete");
                        #endregion

                        read_data = "\0";
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(serialport.ReadExisting());
                    }
                }
                else if (write_data == "z")
                {
                    try
                    {   
                         while (true) {

                            Console.WriteLine("Press q to quit");
                            write_data = (Console.ReadKey().KeyChar).ToString();

                            if (write_data == "q") { break; }
                            else {
                                serialport.WriteLine(write_data);
                            }
                        }
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(serialport.ReadExisting());
                    }
                   
                }
                else if(write_data == "h")
                {
                    try
                    {   
                        Console.WriteLine("Engage UGV to find its path to the destination");
                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(serialport.ReadExisting());
                    }
                }

                else if (write_data == "q")
                {
                    Console.WriteLine("Serial Communication Terminated");
                    serialport.Close();
                }
                Console.Out.Flush();
            }
        }

        private static void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            SerialPort sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            Console.Write(indata);
        }
    }
}
