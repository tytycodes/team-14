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

    def set_players(self, playerA, playerB):
        " connects players with the environment "
        self.playerA = playerA
        self.playerB = playerB
        self.reset()  # defines current_player
        self.score_board = {'X': 0, 'O': 0}

    def reset(self):
        self.turn = 'X'  # the board always starts with X, regardless of which player

        # board states are a 16-character representing the state of the board.
        self.board = list('----------------')
        self.score_board = {'X': 0, 'O': 0}
        if (self.playerA and self.playerB):  # if they are set
            self.playerA.reset_past()
            self.playerB.reset_past()
            if (random.random() < 0.5):  # randomly pick the player to start
                self.current_player = self.playerA
            else:
                self.current_player = self.playerB

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
        if (self.current_player == self.playerA):
            return self.playerB
        else:
            return self.playerA

    def available_actions(self):
        return [ind for ind, val in enumerate(self.board[:12]) if val == '-']

    def other_turn(self):
        return 'X' if self.turn == 'O' else 'O'

    def play_game(self):
        # returns the winning player or None if a tie
        self.reset()
        while True:
            choice = self.current_player.select_action()

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

        if self.score_board[self.turn] > self.score_board[self.other_turn()]:
            self.current_player.reward(100)
            self.other_player().reward(-100)
            return self.current_player
        elif self.score_board[self.turn] < self.score_board[self.other_turn()]:
            self.other_player().reward(100)
            self.current_player.reward(-100)
            return self.other_player()
        else:# it's a tie
            self.current_player.reward(0)
            self.other_player().reward(0)
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


'''
class Agent:
    """ this class is a generic Q-Learning reinforcement learning agent for discrete states and fixed actions
    represented as strings"""
    def __init__(self, environment, difficulty):
        tempdict = ''
        with open(difficulty, 'r') as f:
            for i in f.readlines():
                tempdict = i
        tempdict = eval(tempdict)
        self.Q = defaultdict(lambda: 0.0, tempdict)
        self.environment = environment
        self.reset_past()

    def reset_past(self):
      self.past_action = None
      self.past_state = None

    def select_action(self):
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


def select_difficulty():
    x = 0
    diffdict = {1: r'easy.txt',
                2: r'medium.txt',
                3: r'hard.txt'}
    while(x > 3 or x < 1):
        print("Select a difficulty:")
        print("1: Easy")
        print("2: Medium")
        print("3: Hard")
        x = int(input())

    return diffdict[x]
'''


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
        print(self.history[-games_to_play:].count('B'),'games won by player B')
        print(self.history[-games_to_play:].count('-'),'ties')
        win_rate=self.history[-games_to_play:].count('A')/len(self.history[-games_to_play:])*100
        print("Winning rate: {}".format(win_rate))


board = BoardEnvironment()
A = Agent(board, 'max')
B = Agent(board, 'random')
board.set_players(A,B)

tournament = RepeatedGames(board,A,B)
tournament.play_games(100)
