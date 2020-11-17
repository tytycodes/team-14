import random as rand
import copy
class BoardEnvironment:
	def __init__(self):
		self.reset()
	def reset(self):
		self.turn = 'X' # the board always starts with X, regardless of which player
		# board states are a 42-character representing the state of the board.
		self.board = list('-------------------------')
		if(rand.random() < 0.5):
			return True
		return False
	def get_state(self):
		return "".join(self.board)
	def print_options(self):
		temp_board = copy.copy(self.board)
		for col in range(5):
			bottom = self.get_lowest_column(col)
			if(bottom != -1):
				temp_board[bottom] = col + 1
	def get_lowest_column(self, i):
		if(self.board[i] == '-'):
			while(i + 5 < 25):
				if(self.board[i+5] == '-'):
					i = i + 5
				else:
					break
		else:
			return -1
		return i
	def select_piece(self, choice, turn):
		self.board[choice] = turn
		self.turn = 'X' if (turn == 'O') else 'O'
	def available_actions(self, first):
		movelist = []
		for i in range(5):
			if self.board[i] == '-':
				movelist.append(i)
		return movelist
	def winner(self, check_for = ['X', 'O']):
		straight_lines = (
		(0,1,2,3),
		(1,2,3,4),
		(5,6,7,8),
		(6,7,8,9),
		(10,11,12,13),
		(11,12,13,14),
		(15,16,17,18),
		(16,17,18,19),
		(20,21,22,23),
		(21,22,23,24),
		
		(0,6,12,18),
		(6,12,18,24),
		(1,7,13,19),
		(5,11,17,23),
		
		(3,7,11,15),
		(4,8,12,16),
		(8,12,16,20),
		(9,13,17,21),
		
		(0,5,10,15),
		(5,10,15,20),
		(1,6,11,16),
		(6,11,16,21),
		(2,7,12,17),
		(7,12,17,22),
		(3,8,13,18),
		(8,13,18,23),
		(4,9,14,19),
		(9,14,19,24))
		for turn in check_for:
			for line in straight_lines:
				if all(x == turn for x in (self.board[i] for i in line)):
					return turn
		return '' # if there is no winner
	def is_full(self):
		return('-' not in self.board)