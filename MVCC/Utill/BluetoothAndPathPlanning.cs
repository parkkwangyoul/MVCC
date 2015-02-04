using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;
using System.IO.Ports;

using MVCC.Model;

namespace MVCC.Utill
{
    class BluetoothAndPathPlanning
    {
        private UGV ugv;
        private State state;

        private Globals globals = Globals.Instance;

        private int a;

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

        public static int current_perspective = 0;
        public static int next_perspective = 0;

        public static int[] follow_command = new int[24 + 40];
        public static int[] movement = new int[24 + 40];
        public static int path_count = 0;
        public static int path_num = 0;

        public static int movement_count = 0;

        public static int start_x = 0;
        public static int start_y = 0;
        public static int dest_x = 0;
        public static int dest_y = 0;
        public static int size_ = 0; 
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
                catch (IndexOutOfRangeException) { Console.WriteLine("a"); return false; }
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
                catch (IndexOutOfRangeException) { Console.WriteLine("a"); return false; }
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
                catch (IndexOutOfRangeException) { Console.WriteLine("a"); return false; }
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
                catch (IndexOutOfRangeException) { Console.WriteLine("a"); return false; }
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

         // Movement Command
        public static void follow_path(int start_point_x, int start_point_y, int dest_point_x, int dest_point_y)
        {

            int relative_position_x = dest_point_x - start_point_x;
            int relative_position_y = dest_point_y - start_point_y;

            int current_weight = result_grid[start_point_y,start_point_x];

            //grid[start_point_y,start_point_x] = 5;

            if ((relative_position_x == 0) && (relative_position_y == 0))
            {

                //grid[dest_point_y,dest_point_x] = 5;

                return;
            }
            // Case 1 : 우측 아래 도착점에서 좌측 위 시작점으로 거슬러 가는 경우
            else if ((relative_position_x <= 0) && (relative_position_y <= 0))
            {
                try
                {
                    ////////////////////////////////////////////////////////////////////////////////
                    if ((result_grid[start_point_y - 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x - 1) >= 0))
                    {

                        follow_command[current_weight] = 3; //7 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0))
                    {

                        follow_command[current_weight] = 2; //6 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y - 1, start_point_x] == (current_weight - 1)) && ((start_point_y - 1) >= 0))
                    {

                        follow_command[current_weight] = 4; //0 + 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y - 1, dest_point_x, dest_point_y);
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    else if ((result_grid[start_point_y - 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 5; //1 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y, start_point_x + 1] == (current_weight - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 6; //2 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y + 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 7; //3 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y + 1, start_point_x] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)))
                    {

                        follow_command[current_weight] = 0; //4 - 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y + 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0) && ((start_point_y + 1) <= (row - 1)))
                    {

                        follow_command[current_weight] = 1; //5 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                }
                catch(IndexOutOfRangeException) { }
            }
            // Case 2 : 좌측 위 도착점에서 우측 아래 시작점으로 거슬러 가는 경우
            else if ((relative_position_x >= 0) && (relative_position_y >= 0))
            {
                try
                {
                    ////////////////////////////////////////////////////////////////////////////////
                    if ((result_grid[start_point_y + 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 7; //3 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y, start_point_x + 1] == (current_weight - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 6; //2 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y + 1, start_point_x] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)))
                    {

                        follow_command[current_weight] = 0; //4 - 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y + 1, dest_point_x, dest_point_y);
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    else if ((result_grid[start_point_y + 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0) && ((start_point_y + 1) <= (row - 1)))
                    {

                        follow_command[current_weight] = 1; // 5 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0))
                    {

                        follow_command[current_weight] = 2; //6 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y - 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x - 1) >= 0))
                    {

                        follow_command[current_weight] = 3; //7 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y - 1, start_point_x] == (current_weight - 1)) && ((start_point_y - 1) >= 0))
                    {

                        follow_command[current_weight] = 4; //0 + 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y - 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 5; //1 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                }
                catch { }
            }
            // Case 3 : 좌측 아래 도착점에서 우측 위 시작점으로 거슬러 가는 경우
            else if ((relative_position_x <= 0) && (relative_position_y >= 0))
            {
                try
                {
                    ////////////////////////////////////////////////////////////////////////////////
                    if ((result_grid[start_point_y - 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 5; //1 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y, start_point_x + 1] == (current_weight - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 6; //2 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y - 1, start_point_x] == (current_weight - 1)) && ((start_point_y - 1) >= 0))
                    {

                        follow_command[current_weight] = 4; //0 + 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y - 1, dest_point_x, dest_point_y);
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    else if ((result_grid[start_point_y + 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 7; //3 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y + 1, start_point_x] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)))
                    {

                        follow_command[current_weight] = 0; //4 - 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y + 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0) && ((start_point_y + 1) <= (row - 1)))
                    {

                        follow_command[current_weight] = 1; //5 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0))
                    {

                        follow_command[current_weight] = 2; //6 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y - 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x - 1) >= 0))
                    {

                        follow_command[current_weight] = 3; //7 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                }
                catch { }
            }
            // Case 4 : 우측 위 도착점에서 좌측 아래 시작점으로 거슬러 가는 경우
            else if ((relative_position_x >= 0) && (relative_position_y <= 0))
            {
                try
                {
                    ////////////////////////////////////////////////////////////////////////////////
                    if ((result_grid[start_point_y + 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0) && ((start_point_y + 1) <= (row - 1)))
                    {

                        follow_command[current_weight] = 1; //5 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0))
                    {

                        follow_command[current_weight] = 2; //6 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y + 1, start_point_x] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)))
                    {

                        follow_command[current_weight] = 0; //4 - 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y + 1, dest_point_x, dest_point_y);
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    else if ((result_grid[start_point_y - 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x - 1) >= 0))
                    {

                        follow_command[current_weight] = 3; //7 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y - 1, start_point_x] == (current_weight - 1)) && ((start_point_y - 1) >= 0))
                    {

                        follow_command[current_weight] = 4; //0 + 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y - 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 5; //1 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y, start_point_x + 1] == (current_weight - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 6; //2 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((result_grid[start_point_y + 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        follow_command[current_weight] = 7; //3 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                }
                catch { }
            }
        }

        public static int path_following(int current_direction, int next_direction)
        {

            int degree = 0;

            degree = next_direction - current_direction;

            return degree;
        }

        public static void Movement_Command()
        {

            if (path_following(follow_command[path_num], follow_command[path_num + 1]) == 0)
            {

                movement[path_num] = 0;
                path_num++;

            }
            else if ((path_following(follow_command[path_num], follow_command[path_num + 1]) == 1))
            { // || (path_following(follow_command[path_num], follow_command[path_num+1]) == -1) ){

                movement[path_num] = 1;
                path_num++;

            }
            else if ((path_following(follow_command[path_num], follow_command[path_num + 1]) == 2) || (path_following(follow_command[path_num], follow_command[path_num + 1]) == -6))
            {

                movement[path_num] = 2;
                path_num++;

            }
            else if ((path_following(follow_command[path_num], follow_command[path_num + 1]) == 3) || (path_following(follow_command[path_num], follow_command[path_num + 1]) == -5))
            {

                movement[path_num] = 3;
                path_num++;

            }
            else if ((path_following(follow_command[path_num], follow_command[path_num + 1]) == 4) || (path_following(follow_command[path_num], follow_command[path_num + 1]) == -4))
            {

                movement[path_num] = 4;
                path_num++;

            }
            else if ((path_following(follow_command[path_num], follow_command[path_num + 1]) == 5) || (path_following(follow_command[path_num], follow_command[path_num + 1]) == -3))
            {

                movement[path_num] = 5;
                path_num++;

            }
            else if ((path_following(follow_command[path_num], follow_command[path_num + 1]) == 6) || (path_following(follow_command[path_num], follow_command[path_num + 1]) == -2))
            {

                movement[path_num] = 6;
                path_num++;

            }

            else if ((path_following(follow_command[path_num], follow_command[path_num + 1]) == 7) || (path_following(follow_command[path_num], follow_command[path_num + 1]) == -1))
            {

                movement[path_num] = 7;
                path_num++;

            }

            else if ((path_following(follow_command[path_num], follow_command[path_num + 1]) == -7) && (follow_command[path_num] == 7) && (follow_command[path_num + 1] == 0))
            {

                movement[path_num] = 8;
                path_num++;

            }

            return;
        }

        #endregion

        public void connect(UGV ugv, State state)
        {
            this.ugv = ugv;
            this.state = state;

            //색 트레킹 쓰레드
            BackgroundWorker thread = new BackgroundWorker();
            thread.DoWork += bluetoothConnect;
            thread.RunWorkerAsync();
        }

        private void bluetoothConnect(object sender, DoWorkEventArgs e)
        {
            
            string write_data;
            string read_data;

            string input_grid;

            SerialPort serialport = new SerialPort();

            UGV settingUGV = globals.UGVSettingDictionary[convertId(ugv.Id)];
            

            serialport.PortName = settingUGV.ComPort;
            serialport.BaudRate = settingUGV.Baudrate;
            serialport.DataBits = settingUGV.Databit;
            serialport.StopBits = getStopBit(settingUGV.Stopbit);
            serialport.ReadTimeout = 200;
            serialport.WriteTimeout = 200;

            serialport.Open();

            int[,] map = {{0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,1,1,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,1,1,1,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,2,2,2,2,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0},
                        {0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0,0}};

            if (serialport.IsOpen)
            {
                ugv.IsBluetoothConnected = true;
                state.BluetoothOnOff = true;
            }

            while (serialport.IsOpen)
            {
                bool check = true;

                while (globals.mutex) ;

                globals.mutex = true;

                for (i = 0; i < 24; i++ ) {

                    for (j = 0; j < 40; j++ )
                    {
                        grid[i,j] = '0';
                    }
                }

                for (i = 0; i < 24; i++)
                {
                    for (j = 0; j < 40; j++)
                    {

                        grid[i, j] = globals.Map_obstacle[i, j].ToString()[0];

                        if (check == true && globals.Map_obstacle[i, j] == 2)
                        {

                            start_x = j; 
                            start_y = i;

                            dest_x = 35;
                            dest_y = 3;

                            size_ = 4;

                            check = false;
                        }
                        if (globals.Map_obstacle[i, j] == 2) {

                            grid[i, j] = '0';
                        }
                    }
                }

                for (i = 0; i < 24; i++)
                {
                    for (j = 0; j < 40; j++)
                    {
                        Console.Write("{0} ", grid[i,j]);
                    }
                    Console.WriteLine(" ");
                }

                globals.mutex = false;

                write_data = "f";

                serialport.WriteLine((write_data[0]).ToString());
                /*
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
                */
                if (write_data == "f")
                {
                    try
                    {

                        #region Start_Destination_Size Declaration

                        string starting_point_x;
                        string starting_point_y;
                        string destination_x;
                        string destination_y;
                        string size;

                        char[] start_x_ = new char[3] { '0', '0', '0' };
                        char[] start_y_ = new char[3] { '0', '0', '0' };
                        char[] dest_x_ = new char[3] { '0', '0', '0' };
                        char[] dest_y_ = new char[3] { '0', '0', '0' };
                        char[] s_ = new char[3] { '0', '0', '0' };
                        /*
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
                        */
                        start_x_[2] = (char)((start_x / 100) + 48);         // Hundredth
                        start_x_[1] = (char)(((start_x / 10) % 10) + 48);     // Tenth
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

                                result_grid[i, j] = temp_grid[i, j];
                            }
                        }
                        #endregion

                        #region Graph_Construction

                        graph_reconstruct(node, dest_x, dest_y, start_x, start_y);


                        Console.WriteLine("Graph Reconstruction Complete");
                        #endregion

                        #region BFS Path Planning

                        find_path_BFS(vehicle_1, dest_x, dest_y);

                        for (i = 0; i < 24; i++)
                        {
                            for (j = 0; j < 40; j++)
                            {
                                Console.Write("{0,3} ", result_grid[i, j].ToString());
                            }
                            Console.WriteLine(" ");
                        }

                        #endregion

                        #region Path_Following

                        path_num = 0;
                        path_count = 0;

                        follow_path(dest_x, dest_y, start_x, start_y);
                        follow_command[0] = current_perspective;

                        for (i = 0; i < path_count; i++)
                        {
                            Movement_Command();
                        }

                        for (i = 0; i < path_count; i++)
                        {
                            Console.Write("{0}", follow_command[i]);
                        }
                        Console.WriteLine("");

                        for (i = 0; i < path_count; i++)
                        {
                            Console.Write("{0}", movement[i].ToString());
                        }
                        Console.WriteLine("");

                        #endregion

                        #region Transmit_Movement_Command
                        for (i = 0; i < path_count; i++)
                        {
                            serialport.WriteLine((movement[i]).ToString());
                        }
                        serialport.WriteLine("e");

                        Console.WriteLine("TX Complete");
                        #endregion

//                        write_data = "i";

//                        serialport.WriteLine(write_data[0].ToString());

                        //write_data = "q";

                        serialport.Close();
                        ugv.IsBluetoothConnected = false;
                        state.BluetoothOnOff = false;

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

                    }
                    catch (TimeoutException)
                    {
                        Console.WriteLine("TimeOutException");

                        Console.Write("Buffer : ");
                        Console.WriteLine(serialport.ReadExisting());
                    }
                }
                else if(write_data == "i"){

                }

                else if (write_data == "q")
                {
                    serialport.Close();
                    ugv.IsBluetoothConnected = false;
                    state.BluetoothOnOff = false;
                }
                Console.Out.Flush();
            }
        }

        private StopBits getStopBit(int bit)
        {
            if (bit == 0)
                return StopBits.None;
            else if (bit == 1)
                return StopBits.One;

            return StopBits.One;
        }

        private string convertId(string id)
        {
            switch (id)
            {
                case "A0":
                    return "Vehicle 0";
                case "A1":
                    return "Vehicle 1";
                case "A2":
                    return "Vehicle 2";
                case "A3":
                    return "Vehicle 3";

                default:
                    return "Vehicle 0";
            }
        }
    }
}
