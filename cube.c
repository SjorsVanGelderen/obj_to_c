f32 cubeVertices[] ATTRIBUTE_ALIGN(32) = {
    -1.00F, -1.00F,  1.00F,
    -1.00F,  1.00F,  1.00F,
    -1.00F, -1.00F, -1.00F,
    -1.00F,  1.00F, -1.00F,
     1.00F, -1.00F,  1.00F,
     1.00F,  1.00F,  1.00F,
     1.00F, -1.00F, -1.00F,
     1.00F,  1.00F, -1.00F,
    -1.00F,  0.00F,  0.00F,
     0.00F,  0.00F, -1.00F,
     1.00F,  0.00F,  0.00F,
     0.00F,  0.00F,  1.00F,
     0.00F, -1.00F,  0.00F,
     0.00F,  1.00F,  0.00F
};

u16 cubeIndices[] ATTRIBUTE_ALIGN(32) = {
        1,     2,     0, 
        3,     6,     2, 
        7,     4,     6, 
        5,     0,     4, 
        6,     0,     2, 
        3,     5,     7, 
        1,     3,     2, 
        3,     7,     6, 
        7,     5,     4, 
        5,     1,     0, 
        6,     4,     0, 
        3,     1,     5
};

f32 cubeTexCoords[] ATTRIBUTE_ALIGN(32) = {
    1.00000F, 0.00000F, 
    0.00000F, 1.00000F, 
    0.00000F, 0.00000F, 
    1.00000F, 0.00000F, 
    0.00000F, 1.00000F, 
    0.00000F, 0.00000F, 
    1.00000F, 0.00000F, 
    0.00000F, 1.00000F, 
    0.00000F, 0.00000F, 
    1.00000F, 0.00000F, 
    0.00000F, 1.00000F, 
    0.00000F, 0.00000F, 
    1.00000F, 0.00000F, 
    0.00000F, 1.00000F, 
    0.00000F, 0.00000F, 
    1.00000F, 1.00000F, 
    1.00000F, 1.00000F, 
    1.00000F, 1.00000F, 
    1.00000F, 1.00000F, 
    1.00000F, 1.00000F,
};

u16 cubeTexIndices[] ATTRIBUTE_ALIGN(32) = {
        0,     1,     2, 
        3,     4,     5, 
        6,     7,     8, 
        9,    10,    11, 
       12,    10,     5, 
        3,    13,    14, 
        0,    15,     1, 
        3,    16,     4, 
        6,    17,     7, 
        9,    18,    10, 
       12,    19,    10, 
        3,    18,    13
};

struct Model cube = {
    .vertices         = cubeVertices,
    .verticesAmount   = 14,
    .indices          = cubeIndices,
    .indicesAmount    = 36,
    .texCoords        = cubeTexCoords,
    .texCoordsAmount  = 40,
    .texIndices       = cubeTexIndices,
    .texIndicesAmount = 36
};