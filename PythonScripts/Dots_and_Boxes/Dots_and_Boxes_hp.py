import random
import matplotlib
import copy
import numpy as np
import time
from collections import defaultdict


class BoardEnvironment:
    """ this class creates an environment for agents to interact with"""

    def __init__(self):
        "initialize board"

    def set_players(self, playerA):
        " connects players with the environment "
        self.playerA = playerA
        self.reset()  # defines current_player
        self.score_board = {'X': 0, 'O': 0}

    def reset(self):
        self.turn = 'X'  # the board always starts with X, regardless of which player

        # board states are a 16-character representing the state of the board.
        self.board = list('----------------')
        self.score_board = {'X': 0, 'O': 0}
        if (self.playerA):  # if they are set
            self.playerA.reset_past()
            if (random.random() < 0.5):  # randomly pick the player to start
                self.current_player = True
            else:
                self.current_player = False

    def print_board(self, board_string=None):
        "print more readable board either from supplied board string or the current board"
        if not board_string:
            B = self.board
        else:
            B = board_string

        print('*',B[0],'*',B[1],'*')
        print(B[2],B[12],B[3],B[13],B[4])
        print('*',B[5],'*',B[6],'*')
        print(B[7],B[14],B[8],B[15],B[9])
        print('*',B[10],'*',B[11],'*')


    def get_state(self):
        return "".join(self.board)

    def other_player(self):
        # note, returns other player even if playerA is playing itself
        return not self.current_player


    def available_actions(self):
        return [ind for ind, val in enumerate(self.board[:12]) if val == '-']

    def other_turn(self):
        return 'X' if self.turn == 'O' else 'O'

    def play_game(self):
        # returns the winning player or None if a tie
        self.reset()
        while True:

            print("here")

            # ************ HUMAN-PLAYABLE MODIFICATION
            if(self.current_player):
                choice = self.playerA.select_action()
            else:
                print("SCOREBOARD", self.score_board)
                print("Your board piece is ", self.turn)
                print("Draw a line between two dots")
                movelist = self.available_actions()
                self.print_board()


                x = int(input())
                while(x not in movelist):
                    print("Invalid choice. Please select another board piece.")
                    x = int(input())
                print()
                choice = x
            # **********************************************


            self.board[choice] = self.turn  # should check if valid

            score = self.winner(choice)
            self.score_board[self.turn] += len(score)
            if len(score)!=0:
                #not a tie
#                 self.current_player.reward(100)
                for i in score:
                    self.board[i]=self.turn
            else:
                # switch players
                self.turn = self.other_turn()
                self.current_player = self.other_player()

            if self.is_full():
                break

        # Also added the scoreboard to display the final score
        if self.score_board[self.turn] > self.score_board[self.other_turn()]:
            print(self.turn, "won!")
            print("FINAL SCORE", self.score_board)
            self.print_board()
            return self.current_player
        elif self.score_board[self.turn] < self.score_board[self.other_turn()]:
            print(self.turn, "lost!")
            print("FINAL SCORE", self.score_board)
            self.print_board()
            return self.other_player()
        else:# it's a tie
            print("FINAL SCORE", self.score_board)
            self.print_board()
            return None

    def winner(self, choice):
        boxes = (
            (0, 2, 3, 5),
            (1, 3, 4, 6),
            (5, 7, 8, 10),
            (6, 8, 9, 11)
        )
        result=[]
        for box in boxes:
            if (choice in box) and all(self.board[i] != '-' for i in box):
                if box == (0,2,3,5):
                    result.append(12)
                elif box == (1,3,4,6):
                    result.append(13)
                elif box == (5,7,8,10):
                    result.append(14)
                else:
                    result.append(15)

        return result  # if there is no winner

    def is_full(self):
        return ('-' not in self.board[0:12])
    # %%

    def instructions(self):
        print("Instructions:")
        print("To draw a line, enter the number of the corresponding line")
        print("*",str(0),"*",str(1),"*")
        print(str(2)," ",str(3)," ",str(4))
        print("*",str(5),"*",str(6),"*")
        print(str(7)," ",str(8)," ",str(9))
        print("*",str(10),"*",str(11),"*")



class Agent:
    """ this class is a generic Q-Learning reinforcement learning agent for discrete states and fixed actions
    represented as strings"""
    def __init__(self, environment, policy = 'max', learning_rate = 0.5, discount_factor = 0.95, epsilon = 0.2):
        if policy in ['max', 'random', 'epsilon']:
          self.policy = policy
        else:
          raise InputError(policy, ' is not an available policy')
        self.learning_rate = learning_rate
        self.discount_factor = discount_factor
        self.Q = defaultdict(lambda: 0.0) # stores (state, action) value tuples as keys
        self.environment = environment
        self.epsilon = epsilon # Fraction of time making a random choice for epsilon policy
        self.reset_past()

    def reset_past(self):
        self.past_action = None
        self.past_state = None

    def select_action(self):
        print("selecting action...")
        available_actions = self.environment.available_actions()
        if (self.policy == 'random') or (self.policy == 'epsilon' and random.random() < self.epsilon):
            choice = random.choice(available_actions)
        else: #self.policy == 'max' or it's an epsilon policy determined to pick the max
            Q_vals = [self.Q[(self.environment.get_state(), x)] for x in available_actions]
            #randomly pick one of the maximum values
            max_val = max(Q_vals) # will often be 0 in the beginning
            max_pos = [i for i, j in enumerate(Q_vals) if j == max_val]
            max_indices = [available_actions[x] for x in max_pos]
            choice = random.choice(max_indices)
        self.past_state = self.environment.get_state()
        self.past_action = choice
        return choice

    def reward(self, reward_value):
        # finding the best expected reward
        available_actions = self.environment.available_actions()
        next_Q_vals = [self.Q[(self.environment.get_state(), x)] for x in available_actions]
        max_next_Q = max(next_Q_vals) if next_Q_vals else 0 # will often be 0 in the beginning
        td_target = reward_value + self.discount_factor * max_next_Q
        reward_pred_error = td_target - self.Q[(self.past_state,self.past_action)]
        if (self.past_state or self.past_action):
            self.Q[(self.past_state,self.past_action)] += self.learning_rate * reward_pred_error


import sys
class RepeatedGames:
    def __init__(self, environment, playerA, playerB):
        self.environment = environment
        self.playerA = playerA
        self.playerB = playerB
        self.reset_history()

    def reset_history(self):
        self.history = []

    def play_game(self):
        winner = self.environment.play_game()
        if (winner == self.playerA):
          self.history.append('A')
        elif (winner == self.playerB):
          self.history.append('B')
        else:
          self.history.append('-')

    def play_games(self, games_to_play):
        for i in range(games_to_play):
            self.play_game()
            sys.stdout.write("\r")
            sys.stdout.write("{:2d} games played.".format(i))
            sys.stdout.flush()
        print(self.history[-games_to_play:].count('A'),'games won by player A')
        #print(self.history[-games_to_play:].count('B'),'games won by player B')
        print(self.history[-games_to_play:].count('-'),'ties')
        win_rate=self.history[-games_to_play:].count('A')/len(self.history[-games_to_play:])*100
        print("Winning rate: {}".format(win_rate))


board = BoardEnvironment()
A = Agent(board, 'max')
board.set_players(A)
board.instructions()
board.play_game()

#tournament = RepeatedGames(board,A,B)
#tournament.play_games(100)
