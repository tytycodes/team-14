import random as rand
from collections import defaultdict

class BoardEnvironment:

    def __init__(self):
        "init board"

    def set_players(self, AI):
        self.AI = AI
        self.reset()

    def reset(self):
        self.turn = 'X'

        self.board = list('---------')
        if(rand.random() < 0.5):
            self.current_player = True
        else:
            self.current_player = False

    def print_board(self, board_string = None):
        if not board_string:
            B = self.board
        else:
            B = board_string
        check_for = ['X', 'O']
        print(B[0] if B[0] in check_for else 1,'|', B[1] if B[1] in check_for else 2,'|', B[2] if B[2] in check_for else 3, sep='')
        print('-----')
        print(B[3] if B[3] in check_for else 4,'|', B[4] if B[4] in check_for else 5,'|', B[5] if B[5] in check_for else 6, sep='')
        print('-----')
        print(B[6] if B[6] in check_for else 7,'|', B[7] if B[7] in check_for else 8,'|', B[8] if B[8] in check_for else 9, sep='')

    def get_state(self):
        return "".join(self.board)

    def other_player(self):
        return not self.current_player

    def available_actions(self):
        return [ind for ind, val in enumerate(self.board) if val == '-']

    def play_game(self):
        self.reset()
        while( not self.is_full() ):

            if( not self.current_player ):
                choice = self.AI.select_action()
            else:
                self.print_board()
                choices = self.available_actions()
                print("Select your space to play. Your pieces are", self.turn + '.', "Current choices are")
                print(list(x+1 for x in choices))
                choice = 10
                while(choice not in choices):
                    choice = input()
                    choice = int(choice) - 1
                    if(choice not in choices):
                        print("Spot not available. Current choices are")
                        print(list(x+1 for x in choices))

            self.board[choice] = self.turn

            if self.winner(self.turn):
                if(self.current_player):
                    print("You won!")
                else:
                    print("You lost!")
                self.print_board()
                return self.current_player

            self.turn = 'X' if self.turn == 'O' else 'O'
            self.current_player = not self.current_player
        self.print_board()
        print("Tie!")

        return None

    def winner(self, check_for = ['X', 'O']):
        straight_lines = ((0,1,2),(3,4,5),(6,7,8),(0,3,6),
                          (1,4,7),(2,5,8),(0,4,8),(2,4,6))
        for turn in check_for:
            for line in straight_lines:
                if all(x == turn for x in (self.board[i] for i in line)):
                    return turn
        return ''

    def is_full(self):
        return('-' not in self.board)

class Agent:
    def __init__(self, environment, difficulty):
        self.environment = environment
        self.Q = ''
        with open(r'easy.txt', 'r') as f:
            for i in f.readlines():
                self.Q = i
        self.Q = eval(self.Q)

    def reset_past(self):
        self.past_action = None
        self.past_state = None

    def select_action(self):
        available_actions = self.environment.available_actions()
        Q_vals = [self.Q[(self.environment.get_state(), x)] for x in available_actions]
        max_val = max(Q_vals)
        max_pos = [i for i, j in enumerate(Q_vals) if j == max_val]
        max_indices = [available_actions[x] for x in max_pos]
        choice = rand.choice(max_indices)
        self.past_state = self.environment.get_state()
        self.past_action = choice
        return choice

board = BoardEnvironment()
AI = Agent(board, 'easy.txt')
board.set_players(AI)
board.play_game()
