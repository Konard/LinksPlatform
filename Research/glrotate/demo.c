
#include <stdio.h> // printf()
#include <stdlib.h> // exit()
#include <math.h> // nearbyint()

#include <GL/gl.h>
#include <GL/glext.h>
#include <GL/glu.h>
//#define GLUT_DISABLE_ATEXIT_HACK
#include <GL/glut.h>
#include <GL/freeglut_ext.h> // вместо atexit()

#include "sbt.h"


int WINDOW_W = 1000;
int WINDOW_H = 800;

double r = 45.0;
double angle = 0.0;

void reshapeWindow(int w, int h) {
  // настройка системы координат
  glViewport (0,0,WINDOW_W,WINDOW_H);
  glMatrixMode(GL_PROJECTION);
  glLoadIdentity();
  gluOrtho2D(0,WINDOW_W,0,WINDOW_H);
  glMatrixMode(GL_MODELVIEW);
  glLoadIdentity();
}

const char *filename_font1 = "LinLibertine_Re-4.7.5.ttf";

void drawWindow() {

  // очистка
  glColor4f(0.0, 0.0, 0.0, 0.0);
  glClear(GL_COLOR_BUFFER_BIT);

  // рисование
  glColor4f(1.0, 1.0, 1.0, 0.0);
  glLineStipple(1, 0xAAAA);
  glEnable(GL_LINE_STIPPLE);
  glBegin(GL_LINE_LOOP);
    glVertex2i ( 10, 10 );
    glVertex2i ( 10, 110 );
    glVertex2i ( 110, 110 );
    glVertex2i ( 110, 10 );
  glEnd();
  glDisable(GL_LINE_STIPPLE);

  glColor4f(1.0, 1.0, 1.0, 0.0);
  glEnable(GL_POINT_SMOOTH);
  glPointSize(7.0); // does not work
  glBegin(GL_LINE_STRIP);
    glVertex2i ( 10+50, 10+50 );
    glVertex2i (
      10+50 + (int)nearbyint(r*cos(angle)),
      10+50 + (int)nearbyint(r*sin(angle))
    );
  glEnd();
  glDisable(GL_POINT_SMOOTH);

  printf("%d %d\n",
      50 + (int)nearbyint(r*cos(angle)),
      50 + (int)nearbyint(r*sin(angle))
  );

  glFlush();
  glutSwapBuffers();
}

void keyWindow (unsigned char k, int x, int y) {
  switch (k) {
    case 0x1B:
      exit(0); // нужно правильно завершить программу!
    case 's': {
      break;
    }
    case 'r': {
      glutPostRedisplay();
      break;
    }
  }
}

void mouseButton(int button, int state, int x, int y) {
  if ((button == GLUT_LEFT_BUTTON) && (state == GLUT_DOWN)) {
  }
  if ((button == GLUT_LEFT_BUTTON) && (state == GLUT_UP)) {
  }

  if ((button == GLUT_RIGHT_BUTTON) && (state == GLUT_DOWN)) {
  }
  if ((button == GLUT_RIGHT_BUTTON) && (state == GLUT_UP)) {
  }
}

void mouseMove(int x, int y) {
}


/*
	Эти функции не нужны
	
void idle() {
  glutPostRedisplay();
}

void visibleWindow(int v) {
  if(v == GLUT_VISIBLE) glutIdleFunc(idle);
  else glutIdleFunc(NULL);
}
*/

void Timer(int extra) {
  glutPostRedisplay();
  glutTimerFunc(50,Timer,0);
  angle += 0.01; // в радианах
}

int onRotate(TNodeIndex *pointerToNodeIndex1, TNodeIndex *pointerToNodeIndex2) {
	return 0;
}

int main (int argc, char **argv) {
  if(argc < 1) {
    printf("Need arguments\n");
    return -1;
  }

	// построение необходимых структур данных
	SBT_SetCallback_OnRotate(onRotate);
	SBT_Add(1);
	SBT_Add(2);
	SBT_Add(3);
	SBT_Add(4);
	SBT_Add(5);


  // инициализация библиотеки
  glutInit(&argc,argv);
  glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGBA);

  // создание окна
  glutInitWindowPosition(50, 50);
  glutInitWindowSize(WINDOW_W, WINDOW_H);
  glutCreateWindow("ROTATE");

  // Настройка выхода
  glutSetOption(GLUT_ACTION_ON_WINDOW_CLOSE, GLUT_ACTION_GLUTMAINLOOP_RETURNS);
  // настройка обработчиков событий
  glutDisplayFunc(drawWindow);
  glutReshapeFunc(reshapeWindow);
  glutKeyboardFunc(keyWindow);
  glutMouseFunc(mouseButton);
  glutMotionFunc(mouseMove);
  glutTimerFunc(0,Timer,0);

  glutMainLoop();

  // Завершить программу
  
  return 0;
}
