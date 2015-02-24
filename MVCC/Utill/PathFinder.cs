using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

using MVCC.ViewModel;
using MVCC.Model;

namespace MVCC.Utill
{
    class PathFinder
    {
        private Globals globals = Globals.Instance;

        private UGV ugv;
        private State state;
        int direct;
        int grid_x;
        int grid_y;



        #region Path_Planning_Part
        
        public void init(){
            grid_x = globals.rect_width / globals.x_grid;
            grid_y = globals.rect_height / globals.y_grid;

            grid = new char[grid_y, grid_x];

            grid_1 = new string[grid_y];

            result_grid = new int[grid_y, grid_x];

            temp_grid = new int[grid_y, grid_x];

            unit_100th = new char[grid_y, grid_x];
            unit_10th = new char[grid_y, grid_x];
            unit_1st = new char[grid_y, grid_x];

            current_perspective = 0;
            next_perspective = 0;

            follow_command = new int[grid_y + grid_x];
            movement = new int[grid_y + grid_x];
            path_count = 0;
            path_num = 0;

            movement_count = 0;

            vehicle_1 = new vehicle();
            vehicle_compare = new vehicle();

            node = new vehicle[grid_y, grid_x];

            q = new vehicle[grid_y * grid_x];

            row = grid_y;
            column = grid_x;

            size = 0;

            q_size = grid_y * grid_x - 1;
            q_level = -1;
            count = 0;
        }


        public int abs(int value)
        {

            if (value >= 0) { return value; }
            else { return value * (-1); }
        }

        public char[,] grid;

        public string[] grid_1;

        public int[,] result_grid;

        public int[,] temp_grid;

        public char[,] unit_100th;
        public char[,] unit_10th;
        public char[,] unit_1st;


        public int current_perspective;
        public int next_perspective;

        public int[] follow_command;
        public int[] movement;
        public int path_count;
        public int path_num;

        public int movement_count;

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

        public vehicle vehicle_1;
        public vehicle vehicle_compare;

        public vehicle[,] node;

        public vehicle[] q;

        public int row;
        public int column;

        public int size;

        ////////////////////////////////


        public int q_size;
        public int q_level;
        ////////////////////////////////

        public int count;
        //*************************//  
        public bool right_move_check(vehicle vehicle_1)
        {

            int i = 0;
            int count = 0;
            int size = vehicle_1.size;

            for (i = 0; i < size; i++)
            {

                if (vehicle_1.outer_2.x + 1 < grid_x && vehicle_1.outer_2.y + i < grid_y)
                {
                    if (grid[vehicle_1.outer_2.y + i, vehicle_1.outer_2.x + 1] == '0') { count++; }
                }
                else { return false; }
            }

            if (count == size)
            {

                return true;
            }
            else { return false; }
        }

        public vehicle right_movement(vehicle vehicle_1)
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

        public bool left_move_check(vehicle vehicle_1)
        {

            int i = 0;
            int count = 0;
            int size = vehicle_1.size;

            for (i = 0; i < size; i++)
            {
                if (vehicle_1.outer_1.x - 1 >= 0 && vehicle_1.outer_1.y + i < grid_y)
                {
                    if (grid[vehicle_1.outer_1.y + i, vehicle_1.outer_1.x - 1] == '0') { count++; }
                }
                else { return false; }
            }

            if (count == size)
            {

                return true;
            }
            else { return false; }
        }

        public vehicle left_movement(vehicle vehicle_1)
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

        public bool up_move_check(vehicle vehicle_1)
        {

            int i = 0;
            int count = 0;
            int size = vehicle_1.size;

            for (i = 0; i < size; i++)
            {
                if (vehicle_1.outer_1.y - 1 >= 0 && vehicle_1.outer_1.x + i < grid_x)
                {
                    if (grid[vehicle_1.outer_1.y - 1, vehicle_1.outer_1.x + i] == '0') { count++; }
                }
                else { return false; }
            }

            if (count == size)
            {

                return true;
            }
            else { return false; }
        }

        public vehicle up_movement(vehicle vehicle_1)
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

        public bool down_move_check(vehicle vehicle_1)
        {

            int i = 0;
            int count = 0;
            int size = vehicle_1.size;

            for (i = 0; i < size; i++)
            {
                if (vehicle_1.outer_3.y + 1 < grid_y && vehicle_1.outer_3.x + i < grid_x)
                {
                    if (grid[vehicle_1.outer_3.y + 1, vehicle_1.outer_3.x + i] == '0') { count++; }
                }
                else { return false; }
            }

            if (count == size)
            {

                return true;
            }
            else { return false; }
        }

        public vehicle down_movement(vehicle vehicle_1)
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

        public bool diagonal_right_up_check(vehicle vehicle_1)
        {

            int size = vehicle_1.size;

            if (vehicle_1.outer_2.x + 1 == grid_x - 1 || vehicle_1.outer_2.y == grid_y - 1) { return false; }

            if (right_move_check(vehicle_1) && up_move_check(vehicle_1) && (grid[vehicle_1.outer_2.y - 1, vehicle_1.outer_2.x + 1] == '0'))
            {
                return true;
            }
            else { return false; }

            return false;
        }

        public vehicle move_diagonal_right_up(vehicle vehicle_1)
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

        public bool diagonal_right_down_check(vehicle vehicle_1)
        {

            int size = vehicle_1.size;

            if (vehicle_1.outer_2.x + 1 == grid_x - 1 || vehicle_1.outer_2.y == grid_y - 1) { return false; }

            if (right_move_check(vehicle_1) && down_move_check(vehicle_1) && (grid[vehicle_1.outer_4.y + 1, vehicle_1.outer_4.x + 1] == '0'))
            {

                return true;
            }
            else { return false; }

            return false;
        }

        public vehicle move_diagonal_right_down(vehicle vehicle_1)
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

        public bool diagonal_left_up_check(vehicle vehicle_1)
        {

            int size = vehicle_1.size;

            if (vehicle_1.outer_2.x + 1 == grid_x - 1 || vehicle_1.outer_2.y == grid_y - 1) { return false; }

            if (left_move_check(vehicle_1) && up_move_check(vehicle_1) && (grid[vehicle_1.outer_1.y - 1, vehicle_1.outer_1.x - 1] == '0'))
            {
                return true;
            }
            else { return false; }

            return false;
        }

        public vehicle move_diagonal_left_up(vehicle vehicle_1)
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

        public bool diagonal_left_down_check(vehicle vehicle_1)
        {

            int size = vehicle_1.size;

            if (vehicle_1.outer_2.x + 1 == grid_x - 1 || vehicle_1.outer_2.y == grid_y - 1) { return false; }

            if (left_move_check(vehicle_1) && down_move_check(vehicle_1) && (grid[vehicle_1.outer_3.y + 1, vehicle_1.outer_3.x - 1] == '0'))
            {

                return true;
            }
            else { return false; }

            return false;
        }

        public vehicle move_diagonal_left_down(vehicle vehicle_1)
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

        public vehicle[,] node_init(vehicle[,] in_node)
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

        public void graph_reconstruct(vehicle[,] in_node, int target_x, int target_y, int start_x, int start_y)
        {

            int i = 0;
            int j = 0;

            vehicle[,] temp_node = in_node;

            for (i = 0; i < grid_y; i++)
            {

                for (j = 0; j < grid_x; j++)
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

        public void find_path_BFS(vehicle node_1, int target_x, int target_y)
        {

            int i = 0;
            int j = 0;
            int l = 0;

            vehicle root;
            vehicle pop;

            i = 0; j = 0;

            root = node[node_1.outer_1.y, node_1.outer_1.x];

            q_level++;		// q_level 0에서 시작 / -1 : q가 비워짐
            q[q_level] = root;

            while (q_level != -1)
            {

                l++;

                pop = q[0];

                node[q[0].outer_1.y, q[0].outer_1.x].visited = 1;
                q_level--;
                for (i = 0; i <= q_level; i++)
                {

                    q[i] = q[i + 1];
                }

                if ((pop.outer_1.x == target_x) && (pop.outer_1.y == target_y))
                {

                    Console.WriteLine("x : {0} y : {1}", pop.outer_1.x, pop.outer_1.y);
                    Console.WriteLine("Vehicle has arrived at the destination");
                    break;
                }
                for (i = 0; i < 8; i++)
                {

                    if ((pop.children[i] != null) && (node[(pop.children[i]).outer_1.y, (pop.children[i]).outer_1.x].visited == 0))
                    {

                        node[(pop.children[i]).outer_1.y, (pop.children[i]).outer_1.x].weight = node[pop.outer_1.y, pop.outer_1.x].weight + 1;

                        q_level++;

                        q[q_level] = node[pop.outer_1.y, pop.outer_1.x].children[i];

                        node[q[q_level].outer_1.y, q[q_level].outer_1.x].visited = 1;
                    }
                }
            }

            for (i = 0; i < row; i++)
            {

                for (j = 0; j < column; j++)
                {

                    result_grid[i, j] = node[i, j].weight;

                }
            }
        }

        // Movement Command
        public void follow_path(int start_point_x, int start_point_y, int dest_point_x, int dest_point_y)
        {

            int relative_position_x = dest_point_x - start_point_x;
            int relative_position_y = dest_point_y - start_point_y;

            int current_weight = result_grid[start_point_y, start_point_x];

            int right = 0;
            int left = 0;
            int up = 0;
            int down = 0;

            int dir_0 = 0;
            int dir_1 = 0;
            int dir_2 = 0;
            int dir_3 = 0;
            int dir_4 = 0;
            int dir_5 = 0;
            int dir_6 = 0;
            int dir_7 = 0;

            vehicle_compare.size = vehicle_1.size;

            vehicle_compare.outer_1.x = start_point_x;
            vehicle_compare.outer_1.y = start_point_y;

            vehicle_compare.outer_2.x = start_point_x + vehicle_1.size - 1;
            vehicle_compare.outer_2.y = start_point_y;

            vehicle_compare.outer_3.x = start_point_x;
            vehicle_compare.outer_3.y = start_point_y + vehicle_1.size - 1;

            vehicle_compare.outer_4.x = start_point_x + vehicle_1.size - 1;
            vehicle_compare.outer_4.y = start_point_y + vehicle_1.size - 1;

            vehicle_compare.visited = 1;

            vehicle_compare.weight = 0;

            Console.Write("{0} ", current_weight);
            grid[start_point_y, start_point_x] = '5';

            ugv.PathList.Add(new KeyValuePair<int, int>((start_point_x)* 15, (start_point_y) * 15));

            if ((relative_position_x == 0) && (relative_position_y == 0))
            {

                grid[dest_point_y, dest_point_x] = '5';

                return;
            }
            // Case 1 : 우측 아래 도착점에서 좌측 위 시작점으로 거슬러 가는 경우
            else if ((relative_position_x <= 0) && (relative_position_y <= 0))
            {
                Console.WriteLine("Case 1");

                try
                {
                    ////////////////////////////////////////////////////////////////////////////////
                    right = start_point_x + 1;
                    left = start_point_x - 1;

                    up = start_point_y - 1;
                    down = start_point_y + 1;

                    if (up >= 0) { dir_0 = 1; }
                    if (right <= column && up >= 0) { dir_1 = 1; }
                    if (right <= column) { dir_2 = 1; }
                    if (right <= column && down <= row) { dir_3 = 1; }
                    if (down <= row) { dir_4 = 1; }
                    if (left >= 0 && down <= row) { dir_5 = 1; }
                    if (left >= 0) { dir_6 = 1; }
                    if (left >= 0 && up >= 0) { dir_7 = 1; }

                    if ((dir_6 == 1) && (grid[start_point_y, start_point_x - 1] != 'x') && (result_grid[start_point_y, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0))
                    {

                        //follow_command[current_weight] = 2; //6 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((dir_0 == 1) && (grid[start_point_y - 1, start_point_x] != 'x') && (result_grid[start_point_y - 1, start_point_x] == (current_weight - 1)) && ((start_point_y - 1) >= 0))
                    {

                        //follow_command[current_weight] = 4; //0 + 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y - 1, dest_point_x, dest_point_y);
                    }

                    else if ((dir_7 == 1) && (grid[start_point_y - 1, start_point_x - 1] != 'x') && (result_grid[start_point_y - 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x - 1) >= 0))
                    {

                        //follow_command[current_weight] = 3; //7 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    else if ((dir_1 == 1) && (grid[start_point_y - 1, start_point_x + 1] != 'x') && (result_grid[start_point_y - 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 5; //1 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_2 == 1) && (grid[start_point_y, start_point_x + 1] != 'x') && (result_grid[start_point_y, start_point_x + 1] == (current_weight - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 6; //2 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((dir_3 == 1) && (grid[start_point_y + 1, start_point_x + 1] != 'x') && (result_grid[start_point_y + 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 7; //3 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_4 == 1) && (grid[start_point_y + 1, start_point_x] != 'x') && (result_grid[start_point_y + 1, start_point_x] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)))
                    {

                        //follow_command[current_weight] = 0; //4 - 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_5 == 1) && (grid[start_point_y + 1, start_point_x - 1] != 'x') && (result_grid[start_point_y + 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0) && ((start_point_y + 1) <= (row - 1)))
                    {

                        //follow_command[current_weight] = 1; //5 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                }
                catch (IndexOutOfRangeException) { Console.WriteLine("Catch"); }
            }
            // Case 2 : 좌측 위 도착점에서 우측 아래 시작점으로 거슬러 가는 경우
            else if ((relative_position_x >= 0) && (relative_position_y >= 0))
            {
                Console.WriteLine("Case 2");

                try
                {
                    ////////////////////////////////////////////////////////////////////////////////
                    right = start_point_x + 1;
                    left = start_point_x - 1;

                    up = start_point_y - 1;
                    down = start_point_y + 1;

                    if (up >= 0) { dir_0 = 1; }
                    if (right <= column && up >= 0) { dir_1 = 1; }
                    if (right <= column) { dir_2 = 1; }
                    if (right <= column && down <= row) { dir_3 = 1; }
                    if (down <= row) { dir_4 = 1; }
                    if (left >= 0 && down <= row) { dir_5 = 1; }
                    if (left >= 0) { dir_6 = 1; }
                    if (left >= 0 && up >= 0) { dir_7 = 1; }

                    if ((dir_2 == 1) && (grid[start_point_y, start_point_x + 1] != 'x') && (result_grid[start_point_y, start_point_x + 1] == (current_weight - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 6; //2 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((dir_4 == 1) && (grid[start_point_y + 1, start_point_x] != 'x') && (result_grid[start_point_y + 1, start_point_x] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)))
                    {

                        //follow_command[current_weight] = 0; //4 - 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_3 == 1) && (grid[start_point_y + 1, start_point_x + 1] != 'x') && (result_grid[start_point_y + 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 7; //3 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    else if ((dir_5 == 1) && (grid[start_point_y + 1, start_point_x - 1] != 'x') && (result_grid[start_point_y + 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0) && ((start_point_y + 1) <= (row - 1)))
                    {

                        //follow_command[current_weight] = 1; // 5 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_6 == 1) && (grid[start_point_y, start_point_x - 1] != 'x') && (result_grid[start_point_y, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0))
                    {

                        //follow_command[current_weight] = 2; //6 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((dir_7 == 1) && (grid[start_point_y - 1, start_point_x - 1] != 'x') && (result_grid[start_point_y - 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x - 1) >= 0))
                    {

                        //follow_command[current_weight] = 3; //7 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_0 == 1) && (grid[start_point_y - 1, start_point_x] != 'x') && (result_grid[start_point_y - 1, start_point_x] == (current_weight - 1)) && ((start_point_y - 1) >= 0))
                    {

                        //follow_command[current_weight] = 4; //0 + 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_1 == 1) && (grid[start_point_y - 1, start_point_x + 1] != 'x') && (result_grid[start_point_y - 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 5; //1 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                }
                catch { Console.WriteLine("Catch"); }
            }
            // Case 3 : 좌측 아래 도착점에서 우측 위 시작점으로 거슬러 가는 경우
            else if ((relative_position_x <= 0) && (relative_position_y >= 0))
            {
                Console.WriteLine("Case 3");

                try
                {
                    ////////////////////////////////////////////////////////////////////////////////
                    right = start_point_x + 1;
                    left = start_point_x - 1;

                    up = start_point_y - 1;
                    down = start_point_y + 1;

                    if (up >= 0) { dir_0 = 1; }
                    if (right <= column && up >= 0) { dir_1 = 1; }
                    if (right <= column) { dir_2 = 1; }
                    if (right <= column && down <= row) { dir_3 = 1; }
                    if (down <= row) { dir_4 = 1; }
                    if (left >= 0 && down <= row) { dir_5 = 1; }
                    if (left >= 0) { dir_6 = 1; }
                    if (left >= 0 && up >= 0) { dir_7 = 1; }

                    if ((dir_2 == 1) && (grid[start_point_y, start_point_x + 1] != 'x') && (result_grid[start_point_y, start_point_x + 1] == (current_weight - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 2; //2 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((dir_0 == 1) && (grid[start_point_y - 1, start_point_x] != 'x') && (result_grid[start_point_y - 1, start_point_x] == (current_weight - 1)) && ((start_point_y - 1) >= 0))
                    {

                        //follow_command[current_weight] = 0; //0 + 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_1 == 1) && (grid[start_point_y - 1, start_point_x + 1] != 'x') && (result_grid[start_point_y - 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 1; //1 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    else if ((dir_3 == 1) && (grid[start_point_y + 1, start_point_x + 1] != 'x') && (result_grid[start_point_y + 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 3; //3 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_4 == 1) && (grid[start_point_y + 1, start_point_x] != 'x') && (result_grid[start_point_y + 1, start_point_x] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)))
                    {

                        //follow_command[current_weight] = 4; //4 - 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_5 == 1) && (grid[start_point_y + 1, start_point_x - 1] != 'x') && (result_grid[start_point_y + 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0) && ((start_point_y + 1) <= (row - 1)))
                    {

                        //follow_command[current_weight] = 5; //5 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_6 == 1) && (grid[start_point_y, start_point_x - 1] != 'x') && (result_grid[start_point_y, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0))
                    {

                        //follow_command[current_weight] = 6; //6 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((dir_7 == 1) && (grid[start_point_y - 1, start_point_x - 1] != 'x') && (result_grid[start_point_y - 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x - 1) >= 0))
                    {

                        //follow_command[current_weight] = 7; //7 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                }
                catch { Console.WriteLine("Catch"); }
            }
            // Case 4 : 우측 위 도착점에서 좌측 아래 시작점으로 거슬러 가는 경우
            else if ((relative_position_x >= 0) && (relative_position_y <= 0))
            {
                Console.WriteLine("Case 4");

                try
                {
                    ////////////////////////////////////////////////////////////////////////////////
                    right = start_point_x + 1;
                    left = start_point_x - 1;

                    up = start_point_y - 1;
                    down = start_point_y + 1;

                    if (up >= 0) { dir_0 = 1; }
                    if (right <= column && up >= 0) { dir_1 = 1; }
                    if (right <= column) { dir_2 = 1; }
                    if (right <= column && down <= row) { dir_3 = 1; }
                    if (down <= row) { dir_4 = 1; }
                    if (left >= 0 && down <= row) { dir_5 = 1; }
                    if (left >= 0) { dir_6 = 1; }
                    if (left >= 0 && up >= 0) { dir_7 = 1; }

                    if ((dir_6 == 1) && (grid[start_point_y, start_point_x - 1] != 'x') && (result_grid[start_point_y, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0))
                    {

                        //follow_command[current_weight] = 6; //6 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((dir_4 == 1) && (grid[start_point_y + 1, start_point_x] != 'x') && (result_grid[start_point_y + 1, start_point_x] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)))
                    {

                        //follow_command[current_weight] = 4; //4 - 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_5 == 1) && (grid[start_point_y + 1, start_point_x - 1] != 'x') && (result_grid[start_point_y + 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_x - 1) >= 0) && ((start_point_y + 1) <= (row - 1)))
                    {

                        //follow_command[current_weight] = 5; //5 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }

                    ////////////////////////////////////////////////////////////////////////////////
                    else if ((dir_7 == 1) && (grid[start_point_y - 1, start_point_x - 1] != 'x') && (result_grid[start_point_y - 1, start_point_x - 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x - 1) >= 0))
                    {

                        //follow_command[current_weight] = 7; //7 - 4;
                        path_count++;
                        follow_path(start_point_x - 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_0 == 1) && (grid[start_point_y - 1, start_point_x] != 'x') && (result_grid[start_point_y - 1, start_point_x] == (current_weight - 1)) && ((start_point_y - 1) >= 0))
                    {

                        //follow_command[current_weight] = 0; //0 + 4;
                        path_count++;
                        follow_path(start_point_x, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_1 == 1) && (grid[start_point_y - 1, start_point_x + 1] != 'x') && (result_grid[start_point_y - 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y - 1) >= 0) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 1; //1 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y - 1, dest_point_x, dest_point_y);
                    }
                    else if ((dir_2 == 1) && (grid[start_point_y, start_point_x + 1] != 'x') && (result_grid[start_point_y, start_point_x + 1] == (current_weight - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 2; //2 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y, dest_point_x, dest_point_y);
                    }
                    else if ((dir_3 == 1) && (grid[start_point_y + 1, start_point_x + 1] != 'x') && (result_grid[start_point_y + 1, start_point_x + 1] == (current_weight - 1)) && ((start_point_y + 1) <= (row - 1)) && ((start_point_x + 1) <= (column - 1)))
                    {

                        //follow_command[current_weight] = 3; //3 + 4;
                        path_count++;
                        follow_path(start_point_x + 1, start_point_y + 1, dest_point_x, dest_point_y);
                    }
                }
                catch { Console.WriteLine("Catch"); }
            }
        }

        public int path_following(int current_direction, int next_direction)
        {

            int degree = 0;

            degree = next_direction - current_direction;

            return degree;
        }

        public void Follow_Command()
        {

            int dx = (ugv.PathList[path_count - path_num - 1].Key / 15) - (ugv.PathList[path_count - path_num].Key / 15);
            int dy = (ugv.PathList[path_count - path_num - 1].Value / 15) - (ugv.PathList[path_count - path_num].Value / 15);

            /*
            Console.WriteLine("ugv.PathList[path_count - path_num].Key = {0}", ugv.PathList[path_count - path_num].Key / 15);
            Console.WriteLine("ugv.PathList[path_count - path_num].Value = {0}", ugv.PathList[path_count - path_num].Value / 15);

            Console.WriteLine("ugv.PathList[path_count - path_num - 1].Key = {0}", ugv.PathList[path_count - path_num - 1].Key / 15);
            Console.WriteLine("ugv.PathList[path_count - path_num].Value = {0}", ugv.PathList[path_count - path_num - 1].Value / 15);
            Console.WriteLine();
            */

            if (dx == 0 && dy == -1) { follow_command[path_num] = 0; }
            if (dx == 1 && dy == -1) { follow_command[path_num] = 1; }
            if (dx == 1 && dy == 0) { follow_command[path_num] = 2; }
            if (dx == 1 && dy == 1) { follow_command[path_num] = 3; }
            if (dx == 0 && dy == 1) { follow_command[path_num] = 4; }
            if (dx == -1 && dy == 1) { follow_command[path_num] = 5; }
            if (dx == -1 && dy == 0) { follow_command[path_num] = 6; }
            if (dx == -1 && dy == -1) { follow_command[path_num] = 7; }

            path_num++;

            return;
        }

        public int start_x;
        public int start_y;
        public int dest_x;
        public int dest_y;
        public int size_;

        

        public void Movement_Command() {

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
        
        }

        #endregion

        public void find_path(UGV ugv, State state)
        {
            this.ugv = ugv;
            this.state = state;

            start_x = state.CurrentPointX / 15;
            start_y = state.CurrentPointY / 15;

            dest_x = state.EndPointX;
            dest_y = state.EndPointY;

            Console.WriteLine("ugv.Id = " + ugv.Id + " start_x = " + start_x + " start_y = " + start_y +" dest_x = " + dest_x + " dest_y = " + dest_y);

            //Console.WriteLine("dest_x = " + dest_x + " dest_y = " + dest_y + " current_perspective =" + current_perspective);
            size_ = 5;
            size = 5;

            map_classification(); //차량 구별한 장애물 맵 세팅

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

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    node[i, j] = new vehicle(size_, 0, j, i, 0);
                    result_grid[i, j] = temp_grid[i, j];
                }
            }

            for (int i = 0; i < row; i++)
            {
                for (int j = 0; j < column; j++)
                {
                    Console.Write("{0} ", grid[i, j]);
                }
                Console.WriteLine();
            }


            //vehicle_1 = vehicle_compare;

            #endregion

            #region Graph_Construction

            graph_reconstruct(node, dest_x, dest_y, start_x, start_y);

            for (int i = 0; i < grid_y; i++)
            {
                for (int j = 0; j < grid_x; j++)
                {
                    //Console.WriteLine("Node[{0},{1}]", i, j);
                    for (int k = 0; k < 8; k++)
                    {
                        if (node[i, j].children[k] != null)
                        {
                            //Console.WriteLine("Child[{0}] : {1} {2}", k, node[i, j].children[k].outer_1.x, node[i, j].children[k].outer_1.y);
                        }
                    }
                }
            }

            Console.WriteLine("Graph Reconstruction Complete");
            #endregion

            #region BFS Path Planning

            find_path_BFS(vehicle_1, dest_x, dest_y);

            for (int i = 0; i < grid_y; i++)
            {
                for (int j = 0; j < grid_x; j++)
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

            for (int i = 0; i < path_count; i++)
            {
                Follow_Command();
            }

          
            int index;
            int.TryParse(ugv.Id[1].ToString(), out index);

            if(path_count == 0)
            {
                Console.WriteLine("길 찾기결과 길 없음!!!!!!!!!!!!!!");
                ugv.MovementCommandList.Clear();
                ugv.PathList.Clear();
                return;
            }

            globals.first_point_x[index] = ugv.PathList[path_count - 1].Key / 15;
            globals.first_point_y[index] = ugv.PathList[path_count - 1].Value / 15;


            path_num = 0;
            for (int i = 0; i < path_count - 1; i++)
            {
                Movement_Command();
            }

            Console.WriteLine("");

            for (int i = 0; i < path_count; i++)
            {
                Console.Write("{0}", follow_command[i]);
            }
            Console.WriteLine("");

            for (int i = 0; i < path_count - 1; i++)
            {
                Console.Write("{0}", movement[i].ToString());
            }
            Console.WriteLine("");

            for (int i = 0; i < path_count - 1; i++)
            {
                ugv.MovementCommandList.Add(movement[i].ToString());
            }

            for (int i = 0; i < path_count - 1; i++)
            {
                Console.Write("{0}", ugv.MovementCommandList[i][0].ToString());
            }
            Console.WriteLine("");

            #endregion


            //UGV_confict_check(); //UGV 경로 충돌 검사  
            //UGV_path_evasion(); //USG 경로 회피

        }


        public void map_classification()
        {

            int index;
            int.TryParse(ugv.Id[1].ToString(), out index);

            direct = globals.direction[index];

            globals.theLock.EnterReadLock(); //critical section start
            
            for (int x = 0; x < globals.rect_width / globals.x_grid; x++)
            {
                for (int y = 0; y < globals.rect_height / globals.y_grid; y++)
                {
                    if (globals.Map_obstacle[y, x] == '*') //장애물은 x 로
                        grid[y, x] = 'x';
                   // else if (globals.Map_obstacle[y, x] == 0)
                    //    grid[y, x] = '0';
                   // else if (globals.Map_obstacle[y, x] == index + 1)
                   //     grid[y, x] = '1';
                    else
                        grid[y, x] = '0';
                }
            }

            globals.theLock.ExitReadLock(); //critical section end
        }

        public void UGV_confict_check()
        {
            //Console.WriteLine("mapViewModel.MVCCItemList.Count = " + mapViewModel.MVCCItemList.Count);
            MapViewModel mapViewModel = new MapViewModel();

            for (int i = 0; i < mapViewModel.MVCCItemList.Count; i++)
            {
                Console.WriteLine("i = " + i + " mapViewModel.MVCCItemList.Count = " + mapViewModel.MVCCItemList.Count);

                if (!(mapViewModel.MVCCItemList[i] is UGV))
                    continue;

                UGV temp_ugv = mapViewModel.MVCCItemList[i] as UGV;

                Console.WriteLine("회피 검사하는데로 들어오니?");

                if (ugv.Id != temp_ugv.Id)
                {
                    if (temp_ugv.PathList.Count != 0)
                    {
                        //List<string> temp_movent = temp_ugv.MovementCommandList;
                        int max_path_count;

                        if (ugv.MovementCommandList.Count > temp_ugv.MovementCommandList.Count)
                            max_path_count = ugv.MovementCommandList.Count;
                        else
                            max_path_count = temp_ugv.MovementCommandList.Count;

                        bool confilt_check = false;
                        int confilt_x;
                        int confilt_y;
                        int confilt_count_first = 0;
                        int confilt_count_second = 0;

                        for (int j = 0; j < ugv.MovementCommandList.Count; j++) //두번째로 가고 있는 차
                        {
                            for (int k = 0; k < temp_ugv.MovementCommandList.Count; k++) //첫번째로 가려고 하는 차
                            {
                                if (ugv.PathList[j].Value == temp_ugv.PathList[k].Value)
                                {
                                    confilt_check = true;
                                    confilt_x = ugv.PathList[j].Key;
                                    confilt_y = ugv.PathList[j].Value;

                                    confilt_count_first = k;
                                    confilt_count_second = j;

                                    Console.WriteLine("경로 충돌한다");
                                    break;
                                }
                            }

                            if (confilt_check == true)
                                break;
                        }

                        if (confilt_check == true)
                        {
                            if (confilt_count_first > confilt_count_second) //첫번재 차량이 만나는 경로가 길었을대 
                            {






                            }
                            else //두번재 차량이 만나는 경로가 길었을대
                            {


                            }
                        }
                        else
                        {

                            Console.WriteLine("경로 충돌안한다");
                            continue;
                        }
                    }
                }
            }
        }

        /*
                public void UGV_path_upgrade(object sender, DoWorkEventArgs e)
                {
                    object object_ugv = e.Argument;
                    UGV temp_ugv = (UGV)object_ugv;

                    int index;
                    int.TryParse(temp_ugv.Id[1].ToString(), out index);
           
                    while(ugv.MovementCommandList.Count != 0)
                    {


                    }
                }

        */

        public void UGV_path_evasion()
        {








        }




    }
}
