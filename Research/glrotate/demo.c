
#include <stdio.h> // printf()
#include <stdlib.h> // exit()

#include <GL/gl.h>
#include <GL/glext.h>
#include <GL/glu.h>
#define GLUT_DISABLE_ATEXIT_HACK
#include <GL/glut.h>
#include <GL/freeglut_ext.h> // вместо atexit()


int WINDOW_W = 1280;
int WINDOW_H = 1024;

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
  // рисование
  glColor3f(0.0,0.0,0.0);
  glLineStipple(1,0xAAAA);
  glEnable(GL_LINE_STIPPLE);
  glBegin(GL_LINES);
    glVertex2i ( 0, 0 );
    glVertex2i ( 0, 10);
    glVertex2i ( 10, 10 );
    glVertex2i ( 10, 0 );
  glEnd();

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
}


int main (int argc, char **argv) {
  if(argc < 1) {
    printf("Need arguments\n");
    return -1;
  }

  // инициализация библиотеки
  glutInit(&argc,argv);
  glutInitDisplayMode(GLUT_DOUBLE | GLUT_RGB);

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
